﻿#region Copyright
//  ***********************************************************************
//  Copyright (c) 2016 Jamie Rees
//  File: LinksConfigurationController.cs
//  Created By: Jamie Rees
// 
//  Permission is hereby granted, free of charge, to any person obtaining
//  a copy of this software and associated documentation files (the
//  "Software"), to deal in the Software without restriction, including
//  without limitation the rights to use, copy, modify, merge, publish,
//  distribute, sublicense, and/or sell copies of the Software, and to
//  permit persons to whom the Software is furnished to do so, subject to
//  the following conditions:
//  
//  The above copyright notice and this permission notice shall be
//  included in all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//  MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//  LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//  OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//  WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ***********************************************************************
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

using NZBDash.Core.Interfaces;
using NZBDash.Core.Model.DTO;
using NZBDash.UI.Models;

namespace NZBDash.UI.Controllers
{
    public class LinksConfigurationController : BaseController
    {
        private ILinksConfiguration Service { get; set; }

        public LinksConfigurationController(ILinksConfiguration service)
            : base(typeof(LinksConfigurationController))
        {
            Service = service;
        }

        // GET: LinksConfiguration
        public ActionResult Index()
        {
            var result = Service.GetLinks();
            if (result == null) return View(new List<LinksViewModel>());

            var model = result.Select(item => new LinksViewModel
            {
                Id = item.Id,
                LinkEndpoint = new Uri(item.LinkEndpoint),
                LinkName = item.LinkName
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateLink(LinksViewModel config)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            var dto = new LinksConfigurationDto { Id = config.Id, LinkName = config.LinkName, LinkEndpoint = config.LinkEndpoint.ToString() };

            var result = Service.UpdateLink(dto);
            if (result)
            {
                return RedirectToAction("Index");
            }

            return View("Error");
        }

        public ActionResult GetLink(int id)
        {
            var links = Service.GetLinks();
            var link = links.FirstOrDefault(x => x.Id == id);
            var model = new LinksViewModel { Id = link.Id, LinkName = link.LinkName, LinkEndpoint = new Uri(link.LinkEndpoint) };
            return PartialView("_Link", model);
        }
    }
}