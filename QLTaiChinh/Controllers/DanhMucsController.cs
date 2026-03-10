using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLTaiChinh.Data;

namespace QLTaiChinh.Controllers
{
    public class DanhMucsController : Controller
    {
        private readonly QuanLyTaiChinhCaNhanContext _context;

        public DanhMucsController(QuanLyTaiChinhCaNhanContext context)
        {
            _context = context;
        }

        // GET: DanhMucs
        public async Task<IActionResult> Index()
        {
            var quanLyTaiChinhCaNhanContext = _context.DanhMucs.Include(d => d.DanhMucCha).Include(d => d.NguoiDung);
            return View(await quanLyTaiChinhCaNhanContext.ToListAsync());
        }

        // GET: DanhMucs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMuc = await _context.DanhMucs
                .Include(d => d.DanhMucCha)
                .Include(d => d.NguoiDung)
                .FirstOrDefaultAsync(m => m.DanhMucId == id);
            if (danhMuc == null)
            {
                return NotFound();
            }

            return View(danhMuc);
        }

        // GET: DanhMucs/Create
        public IActionResult Create()
        {
            ViewData["DanhMucChaId"] = new SelectList(_context.DanhMucs, "DanhMucId", "DanhMucId");
            ViewData["NguoiDungId"] = new SelectList(_context.NguoiDungs, "NguoiDungId", "NguoiDungId");
            return View();
        }

        // POST: DanhMucs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DanhMucId,NguoiDungId,TenDanhMuc,LoaiDanhMuc,DanhMucChaId,TrangThai,NgayTao")] DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(danhMuc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DanhMucChaId"] = new SelectList(_context.DanhMucs, "DanhMucId", "DanhMucId", danhMuc.DanhMucChaId);
            ViewData["NguoiDungId"] = new SelectList(_context.NguoiDungs, "NguoiDungId", "NguoiDungId", danhMuc.NguoiDungId);
            return View(danhMuc);
        }

        // GET: DanhMucs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null)
            {
                return NotFound();
            }
            ViewData["DanhMucChaId"] = new SelectList(_context.DanhMucs, "DanhMucId", "DanhMucId", danhMuc.DanhMucChaId);
            ViewData["NguoiDungId"] = new SelectList(_context.NguoiDungs, "NguoiDungId", "NguoiDungId", danhMuc.NguoiDungId);
            return View(danhMuc);
        }

        // POST: DanhMucs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DanhMucId,NguoiDungId,TenDanhMuc,LoaiDanhMuc,DanhMucChaId,TrangThai,NgayTao")] DanhMuc danhMuc)
        {
            if (id != danhMuc.DanhMucId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhMuc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhMucExists(danhMuc.DanhMucId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DanhMucChaId"] = new SelectList(_context.DanhMucs, "DanhMucId", "DanhMucId", danhMuc.DanhMucChaId);
            ViewData["NguoiDungId"] = new SelectList(_context.NguoiDungs, "NguoiDungId", "NguoiDungId", danhMuc.NguoiDungId);
            return View(danhMuc);
        }

        // GET: DanhMucs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var danhMuc = await _context.DanhMucs
                .Include(d => d.DanhMucCha)
                .Include(d => d.NguoiDung)
                .FirstOrDefaultAsync(m => m.DanhMucId == id);
            if (danhMuc == null)
            {
                return NotFound();
            }

            return View(danhMuc);
        }

        // POST: DanhMucs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc != null)
            {
                _context.DanhMucs.Remove(danhMuc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DanhMucExists(int id)
        {
            return _context.DanhMucs.Any(e => e.DanhMucId == id);
        }
    }
}
