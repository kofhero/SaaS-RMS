﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS_RMS.Data;
using SaaS_RMS.Models.Entities.Inventory;
using SaaS_RMS.Models.Enums;

namespace SaaS_RMS.Controllers.InventoryController
{
    public class StockDetailsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;

        #region Constuctor

        public StockDetailsController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _db = context;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Index

        public async Task<IActionResult> Index()
        {
            var restaurant = _session.GetInt32("restaurantsessionid");

            var stockDetails = await _db.StockDetails.Where(s => s.RestaurantId == restaurant)
                .Include(s => s.Restuarant)
                .Include(s => s.Vendor)
                .Include(s => s.Product)
                .ToListAsync();

            if (stockDetails != null)
            {
                return View(stockDetails);
            }
            else
            {
                return RedirectToAction("Access", "Restaurants");
            }
        }

        #endregion

        #region Create

        //GET: StockDetails/Create
        [HttpGet]
        public IActionResult Create()
        {
            
            var stockDetail = new StockDetail();
            return PartialView("Create", stockDetail);
        }

        #endregion

        #region Delete

        //GET: StockDetails/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockDetail = await _db.StockDetails.SingleOrDefaultAsync(b => b.StockDetailId == id);

            if (stockDetail == null)
            {
                return NotFound();
            }

            return PartialView("Delete", stockDetail);
        }

        //POST:
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stockDetail = await _db.StockDetails.SingleOrDefaultAsync(b => b.StockDetailId == id);
            if (stockDetail != null)
            {
                _db.StockDetails.Remove(stockDetail);
                await _db.SaveChangesAsync();

                TempData["stockDetail"] = "You have successfully deleted " + stockDetail.Product + " Stock Detail!!!";
                TempData["notificationType"] = NotificationType.Success.ToString();

                return Json(new { success = true });
            }
            return RedirectToAction("Index");
        }

        #endregion

        #region Fetch Data

        public JsonResult GetProductsForCategory(int id)
        {
            var products = _db.Products.Where(p => p.CategoryId == id);
            return Json(products);
        }

        #endregion

        #region StockDetials Exists

        private bool StockDetailsExists(int? id)
        {
            return _db.StockDetails.Any(s => s.StockDetailId == id);
        }

        #endregion

        
    }
}