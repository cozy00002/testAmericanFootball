using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAmericanFootball2.Enums;
using TestAmericanFootball2.Extentions;
using TestAmericanFootball2.Models;
using TestAmericanFootball2.Service.Interface;
using TestAmericanFootball2.ViewModels;

namespace TestAmericanFootball2.Controllers
{
    public class AmeFootController : Controller
    {
        private readonly AFDbContext _context;
        private readonly IGameService _service;

        public AmeFootController(AFDbContext context, IGameService service)
        {
            _context = context;
            _service = service;
        }

        #region 作成中

        // GET: AmeFoot
        public ActionResult Index()
        {
            var game = _service.InitializeGame();
            var ameFootVM = Mapper.Map<AmeFootViiewModel>(game);
            return View(ameFootVM);
        }

        // POST: AmeFoot/Game
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Game(string player1Id,
            string player2Id,
            string method,
            bool isAiAuto
            )
        {
            var game = await _service.GameAsync(player1Id, player2Id, method);
            var ameFootVM = Mapper.Map<AmeFootViiewModel>(game);
            ameFootVM.IsAIAuto = isAiAuto;
            return View(ameFootVM);
        }

        #endregion 作成中
    }

    #region ごみ

    //    // GET: AmeFoot/Details/5
    //    public ActionResult Details(int id)
    //    {
    //        return View();
    //    }

    //    // GET: AmeFoot/Create
    //    public ActionResult Create()
    //    {
    //        return View();
    //    }

    //    // POST: AmeFoot/Create
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Create(IFormCollection collection)
    //    {
    //        try
    //        {
    //            // TODO: Add insert logic here

    //            return RedirectToAction(nameof(Index));
    //        }
    //        catch
    //        {
    //            return View();
    //        }
    //    }

    //    // GET: AmeFoot/Edit/5
    //    public ActionResult Edit(int id)
    //    {
    //        return View();
    //    }

    //    // POST: AmeFoot/Edit/5
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Edit(int id, IFormCollection collection)
    //    {
    //        try
    //        {
    //            // TODO: Add update logic here

    //            return RedirectToAction(nameof(Index));
    //        }
    //        catch
    //        {
    //            return View();
    //        }
    //    }

    //    // GET: AmeFoot/Delete/5
    //    public ActionResult Delete(int id)
    //    {
    //        return View();
    //    }

    //    // POST: AmeFoot/Delete/5
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Delete(int id, IFormCollection collection)
    //    {
    //        try
    //        {
    //            // TODO: Add delete logic here

    //            return RedirectToAction(nameof(Index));
    //        }
    //        catch
    //        {
    //            return View();
    //        }
    //    }
    //}

    #endregion ごみ
}