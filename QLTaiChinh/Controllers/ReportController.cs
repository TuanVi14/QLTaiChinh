using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data;
using QLTaiChinh.Data;
using SysFile = System.IO.File;

namespace QLTaiChinh.Controllers
{
    [Authorize]
    public class ReportController : BaseController
    {

        private readonly IConfiguration _configuration;
        private static QuanLyTaiChinhCaNhanContext context;

        public ReportController(IConfiguration configuration):base(context)
        {
            _configuration = configuration;
        }

        // Lấy UserID từ Session (giống LoginController)
        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserID");
        }

        private string GetConnectionString()
            => _configuration.GetConnectionString("DefaultConnection")!;

        // ============================================================
        // INDEX - Trang chọn báo cáo
        // ============================================================
        public IActionResult Report()
        {
            if (GetCurrentUserId() == null)
                return RedirectToAction("Login", "Login");

            ViewBag.CurrentMonth = DateTime.Now.Month;
            ViewBag.CurrentYear = DateTime.Now.Year;
            return View();
        }

        // ============================================================
        // XUẤT EXCEL - Báo cáo Thu Chi theo tháng
        // ============================================================
        [HttpGet]
        public IActionResult ExportExcel(int thang, int nam)
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Login");
            int nguoiDungID = userId.Value;

            var dt = LayDuLieuBaoCao(nguoiDungID, thang, nam);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("BaoCaoThuChi");

            // ==== TIÊU ĐỀ ====
            worksheet.Cell("A1").Value = $"BÁO CÁO THU - CHI THÁNG {thang:D2}/{nam}";
            var titleRange = worksheet.Range("A1:G1");
            titleRange.Merge();
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 16;
            titleRange.Style.Font.FontColor = XLColor.White;
            titleRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2c3e50");
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell("A2").Value = $"Xuất ngày: {DateTime.Now:dd/MM/yyyy HH:mm}";
            var subTitle = worksheet.Range("A2:G2");
            subTitle.Merge();
            subTitle.Style.Font.Italic = true;
            subTitle.Style.Font.FontColor = XLColor.Gray;
            subTitle.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // ==== HEADER BẢNG ====
            int headerRow = 4;
            string[] headers = { "STT", "Ngày GD", "Loại", "Danh mục", "Mô tả", "Hình thức TT", "Số tiền (VNĐ)" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(headerRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#3498db");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // ==== DỮ LIỆU ====
            int dataRow = headerRow + 1;
            int stt = 1;
            decimal tongThu = 0, tongChi = 0;

            foreach (DataRow row in dt.Rows)
            {
                bool isThu = row["LoaiGiaoDich"].ToString() == "Thu";
                decimal soTien = Convert.ToDecimal(row["SoTien"]);

                worksheet.Cell(dataRow, 1).Value = stt++;
                worksheet.Cell(dataRow, 2).Value = Convert.ToDateTime(row["NgayGiaoDich"]).ToString("dd/MM/yyyy");
                worksheet.Cell(dataRow, 3).Value = row["LoaiGiaoDich"].ToString();
                worksheet.Cell(dataRow, 4).Value = row["TenDanhMuc"].ToString();
                worksheet.Cell(dataRow, 5).Value = row["MoTa"].ToString();
                worksheet.Cell(dataRow, 6).Value = row["HinhThucThanhToan"].ToString();

                var soTienCell = worksheet.Cell(dataRow, 7);
                soTienCell.Value = soTien;
                soTienCell.Style.NumberFormat.Format = "#,##0";
                soTienCell.Style.Font.FontColor = isThu ? XLColor.DarkGreen : XLColor.DarkRed;
                soTienCell.Style.Font.Bold = true;

                if (dataRow % 2 == 0)
                    worksheet.Range(dataRow, 1, dataRow, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");

                worksheet.Range(dataRow, 1, dataRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Hair;

                if (isThu) tongThu += soTien;
                else tongChi += soTien;

                dataRow++;
            }

            // ==== TỔNG KẾT ====
            int summaryRow = dataRow + 1;

            var tongThuLabel = worksheet.Cell(summaryRow, 5);
            tongThuLabel.Value = "Tổng Thu:";
            tongThuLabel.Style.Font.Bold = true;
            tongThuLabel.Style.Font.FontColor = XLColor.DarkGreen;
            tongThuLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var tongThuVal = worksheet.Cell(summaryRow, 7);
            tongThuVal.Value = tongThu;
            tongThuVal.Style.NumberFormat.Format = "#,##0";
            tongThuVal.Style.Font.Bold = true;
            tongThuVal.Style.Font.FontColor = XLColor.DarkGreen;

            int chiRow = summaryRow + 1;
            var tongChiLabel = worksheet.Cell(chiRow, 5);
            tongChiLabel.Value = "Tổng Chi:";
            tongChiLabel.Style.Font.Bold = true;
            tongChiLabel.Style.Font.FontColor = XLColor.DarkRed;
            tongChiLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var tongChiVal = worksheet.Cell(chiRow, 7);
            tongChiVal.Value = tongChi;
            tongChiVal.Style.NumberFormat.Format = "#,##0";
            tongChiVal.Style.Font.Bold = true;
            tongChiVal.Style.Font.FontColor = XLColor.DarkRed;

            int canDoiRow = chiRow + 1;
            var canDoiRange = worksheet.Range(canDoiRow, 1, canDoiRow, 7);
            canDoiRange.Merge();
            decimal canDoi = tongThu - tongChi;
            canDoiRange.Value = $"CÂN ĐỐI: {(canDoi >= 0 ? "+" : "")}{canDoi:#,##0} VNĐ";
            canDoiRange.Style.Font.Bold = true;
            canDoiRange.Style.Font.FontSize = 12;
            canDoiRange.Style.Font.FontColor = canDoi >= 0 ? XLColor.DarkGreen : XLColor.DarkRed;
            canDoiRange.Style.Fill.BackgroundColor = canDoi >= 0 ? XLColor.FromHtml("#d4edda") : XLColor.FromHtml("#f8d7da");
            canDoiRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            canDoiRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

            // ==== ĐỘ RỘNG CỘT ====
            worksheet.Column(1).Width = 6;
            worksheet.Column(2).Width = 14;
            worksheet.Column(3).Width = 10;
            worksheet.Column(4).Width = 20;
            worksheet.Column(5).Width = 28;
            worksheet.Column(6).Width = 18;
            worksheet.Column(7).Width = 18;
            worksheet.SheetView.FreezeRows(headerRow);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"BaoCao_ThuChi_{thang:D2}_{nam}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ============================================================
        // XUẤT PDF - Báo cáo Thu Chi theo tháng
        // ============================================================
        [HttpGet]
        public IActionResult ExportPdf(int thang, int nam)
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Login");
            int nguoiDungID = userId.Value;

            var dt = LayDuLieuBaoCao(nguoiDungID, thang, nam);

            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4.Rotate(), 20f, 20f, 30f, 20f);
            var writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            // Font hỗ trợ tiếng Việt
            string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "arial.ttf");
            BaseFont baseFont = SysFile.Exists(fontPath)
                ? BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED)
                : BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

            var fontTitle = new iTextSharp.text.Font(baseFont, 18, iTextSharp.text.Font.BOLD, new BaseColor(44, 62, 80));
            var fontSubtitle = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.ITALIC, BaseColor.GRAY);
            var fontHeader = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.BOLD, BaseColor.WHITE);
            var fontData = new iTextSharp.text.Font(baseFont, 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            var fontThu = new iTextSharp.text.Font(baseFont, 9, iTextSharp.text.Font.BOLD, new BaseColor(27, 94, 32));
            var fontChi = new iTextSharp.text.Font(baseFont, 9, iTextSharp.text.Font.BOLD, new BaseColor(183, 28, 28));

            // ==== TIÊU ĐỀ ====
            document.Add(new Paragraph($"BÁO CÁO THU - CHI THÁNG {thang:D2}/{nam}", fontTitle)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 5f
            });
            document.Add(new Paragraph($"Xuất ngày: {DateTime.Now:dd/MM/yyyy HH:mm}", fontSubtitle)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15f
            });

            // ==== BẢNG DỮ LIỆU ====
            var table = new PdfPTable(7) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 4f, 10f, 8f, 14f, 22f, 13f, 14f });

            var headerBg = new BaseColor(52, 152, 219);
            foreach (var h in new[] { "STT", "Ngày GD", "Loại", "Danh mục", "Mô tả", "Hình thức TT", "Số tiền (VNĐ)" })
            {
                table.AddCell(new PdfPCell(new Phrase(h, fontHeader))
                {
                    BackgroundColor = headerBg,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 6f
                });
            }

            int stt = 1;
            decimal tongThu = 0, tongChi = 0;
            bool isAlternate = false;
            var altBg = new BaseColor(248, 249, 250);

            foreach (DataRow row in dt.Rows)
            {
                bool isThu = row["LoaiGiaoDich"].ToString() == "Thu";
                decimal amt = Convert.ToDecimal(row["SoTien"]);
                var rowBg = isAlternate ? altBg : BaseColor.WHITE;

                void AddCell(string text, iTextSharp.text.Font f, int align = Element.ALIGN_LEFT)
                {
                    table.AddCell(new PdfPCell(new Phrase(text, f))
                    {
                        BackgroundColor = rowBg,
                        HorizontalAlignment = align,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 5f
                    });
                }

                AddCell(stt++.ToString(), fontData, Element.ALIGN_CENTER);
                AddCell(Convert.ToDateTime(row["NgayGiaoDich"]).ToString("dd/MM/yyyy"), fontData, Element.ALIGN_CENTER);
                AddCell(row["LoaiGiaoDich"].ToString()!, isThu ? fontThu : fontChi, Element.ALIGN_CENTER);
                AddCell(row["TenDanhMuc"].ToString()!, fontData);
                AddCell(row["MoTa"].ToString()!, fontData);
                AddCell(row["HinhThucThanhToan"].ToString()!, fontData);
                AddCell($"{amt:#,##0}", isThu ? fontThu : fontChi, Element.ALIGN_RIGHT);

                if (isThu) tongThu += amt;
                else tongChi += amt;

                isAlternate = !isAlternate;
            }

            document.Add(table);
            document.Add(new Paragraph(" "));

            // ==== TỔNG KẾT ====
            var summaryTable = new PdfPTable(2) { WidthPercentage = 40, HorizontalAlignment = Element.ALIGN_RIGHT };
            summaryTable.SetWidths(new float[] { 1f, 1f });

            void AddSummaryRow(string label, decimal value, BaseColor bg, BaseColor textColor)
            {
                var f = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.BOLD, textColor);
                summaryTable.AddCell(new PdfPCell(new Phrase(label, f)) { BackgroundColor = bg, Padding = 6f, HorizontalAlignment = Element.ALIGN_RIGHT });
                summaryTable.AddCell(new PdfPCell(new Phrase($"{value:#,##0} VNĐ", f)) { BackgroundColor = bg, Padding = 6f, HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            AddSummaryRow("Tổng Thu:", tongThu, new BaseColor(212, 237, 218), new BaseColor(27, 94, 32));
            AddSummaryRow("Tổng Chi:", tongChi, new BaseColor(248, 215, 218), new BaseColor(183, 28, 28));
            decimal canDoi = tongThu - tongChi;
            AddSummaryRow("Cân đối:", canDoi,
                canDoi >= 0 ? new BaseColor(212, 237, 218) : new BaseColor(248, 215, 218),
                canDoi >= 0 ? new BaseColor(27, 94, 32) : new BaseColor(183, 28, 28));

            document.Add(summaryTable);
            document.Close();

            string fileName = $"BaoCao_ThuChi_{thang:D2}_{nam}.pdf";
            return File(stream.ToArray(), "application/pdf", fileName);
        }

        // ============================================================
        // HELPER - Lấy dữ liệu từ DB
        // ============================================================
        private DataTable LayDuLieuBaoCao(int nguoiDungID, int thang, int nam)
        {
            var dt = new DataTable();
            string sql = @"
                SELECT g.GiaoDichID, g.LoaiGiaoDich, g.SoTien, g.NgayGiaoDich,
                       g.MoTa, g.HinhThucThanhToan, d.TenDanhMuc
                FROM GiaoDich g
                JOIN DanhMuc d ON g.DanhMucID = d.DanhMucID
                WHERE g.NguoiDungID = @NguoiDungID
                  AND MONTH(g.NgayGiaoDich) = @Thang
                  AND YEAR(g.NgayGiaoDich)  = @Nam
                ORDER BY g.NgayGiaoDich ASC, g.LoaiGiaoDich DESC";

            using var conn = new SqlConnection(GetConnectionString());
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NguoiDungID", nguoiDungID);
            cmd.Parameters.AddWithValue("@Thang", thang);
            cmd.Parameters.AddWithValue("@Nam", nam);

            conn.Open();
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
            return dt;
        }
    }
}
