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

namespace SaaS_RMS.Controllers.InventoryControllers
{
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;

        #region Constructor

        public PurchasesController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _db = context;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Index

        [Route("purchases/index/{PurchaseEntryId}")]
        public async Task<IActionResult> Index(int PurchaseEntryId)
        {
            var restaurant = _session.GetInt32("restaurantsessionid");

            if (restaurant == null)
            {
                return RedirectToAction("Access", "Restaurants");
            }

            var purchase = await _db.Purchases.Include(p => p.StockDetail)
                       .Include(p => p.PurchaseEntry)
                       .ToListAsync();

            if (purchase == null)
            {
                return NotFound();
            }
            else
            {
                _session.SetInt32("purchaseentrysessionid", PurchaseEntryId);
            }

            return View(purchase);
        }

        #endregion

        #region Create

        //GET: Purchases/Create
        [HttpGet]
        public IActionResult Create()
        {
            var purchase = new Purchase();
            return PartialView("Create", purchase);
        }

        //POST:
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Purchase purchase)
        {
            var purchaseentryid = _session.GetInt32("purchaseentrysessionid");

            if (ModelState.IsValid)
            {
                if (purchaseentryid != null)
                {
                    purchase.PurchaseEntryId = Convert.ToInt32(purchaseentryid);

                    await _db.AddAsync(purchase);
                    await _db.SaveChangesAsync();

                    TempData["purchase"] = "You have successfully added " + purchase.StockDetail.Product.Name + " as a new Product!!!";
                    TempData["notificationType"] = NotificationType.Success.ToString();

                    return Json(new { success = true });
                }
            }

            return RedirectToAction("Index", new { PurchaseEntryId = purchaseentryid });
        }

        #endregion

        #region Purchase Exists

        private bool PurchaseExists(int? id)
        {
            return _db.Purchases.Any(p => p.PurchaseEntryId == id);
        }

        #endregion

    }
}