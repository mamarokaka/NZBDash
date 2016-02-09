﻿#region Copyright
// /************************************************************************
//   Copyright (c) 2016 NZBDash
//   File: AlertSettingsController.cs
//   Created By: Jamie Rees
//  
//   Permission is hereby granted, free of charge, to any person obtaining
//   a copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//  
//   The above copyright notice and this permission notice shall be
//   included in all copies or substantial portions of the Software.
//  
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//   EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//   NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//   LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//   OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//   WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using NZBDash.Common.Interfaces;
using NZBDash.Core.Interfaces;
using NZBDash.Core.Models.Settings;
using NZBDash.UI.Models.ViewModels.Settings;

using Omu.ValueInjecter;

namespace NZBDash.UI.Controllers
{
    public class AlertSettingsController : BaseController
    {
        public AlertSettingsController(ILogger logger, ISettingsService<AlertSettingsDto> alertSettings, IHardwareService hardware) : base(logger)
        {
            AlertSettingsService = alertSettings;
            HardwareService = hardware;
        }
        private ISettingsService<AlertSettingsDto> AlertSettingsService { get; set; }
        private IHardwareService HardwareService { get; set; }

        [HttpGet]
        public ActionResult Index()
        {
            var settings = AlertSettingsService.GetSettings();

            var model = Mapper.Map<AlertSettingsViewModel>(settings);

            foreach (var rDto in settings.AlertRules)
            {
                var m = new AlertRules();
                m.InjectFrom(rDto);
                m.NicId = rDto.NicId;

                model.AlertRules.Add(m);
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult UpdateAlert(int id)
        {
            var settings = AlertSettingsService.GetSettings();
            var selected = settings.AlertRules.FirstOrDefault(x => x.Id == id);
            var vm = new AlertRules();

            if (selected != null)
            {
                vm.InjectFrom(selected);
            }

            if (vm.AlertType == AlertType.Cpu)
            {
                return PartialView("CpuAlertModal", vm);
            }
            return View("Error");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult UpdateAlert(AlertRules model)
        {
            if (!model.IsValid)
            {
                return Json(model.Errors);
            }
            var currentSettings = AlertSettingsService.GetSettings();

            var maxRecord = 1;

            if (currentSettings.AlertRules.Count > 0)
            {
                maxRecord = currentSettings.AlertRules.Max(x => x.Id);
            }

            var match = currentSettings.AlertRules.FirstOrDefault(x => x.Id == model.Id);
            if (match == null)  
            {
                // We don't yet have any rules so create one
                var dtoRule = Mapper.Map<AlertRules, AlertRulesDto>(model);
                dtoRule.Id = ++maxRecord;

                
                currentSettings.AlertRules.Add(dtoRule);
                var result = AlertSettingsService.SaveSettings(currentSettings);
                if (result)
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                // We have rules already so let's modify the existing rules, remove the existing and add the new
                var dtoModel = new AlertRulesDto();
                dtoModel.InjectFrom(model);
                currentSettings.AlertRules.Remove(match);
                currentSettings.AlertRules.Add(dtoModel);
                var result = AlertSettingsService.SaveSettings(currentSettings);
                if (result)
                {
                    return RedirectToAction("Index");
                }
            }

            return View("Error");
        }

        public ActionResult DeleteAlert(int id)
        {
            var settings = AlertSettingsService.GetSettings();
            var settingToRemove = settings.AlertRules.FirstOrDefault(x => x.Id == id);

            settings.AlertRules.Remove(settingToRemove);

            var result = AlertSettingsService.SaveSettings(settings);
            if (result)
            {
                return RedirectToAction("Index");
            }

            return View("Error");
        }

        [HttpGet]
        public ActionResult OpenModal(AlertType alertType)
        {
            var model = new AlertRules { AlertType = alertType };

            switch (model.AlertType)
            {
                case AlertType.Cpu:
                    return PartialView("CpuAlertModal", model);
                case AlertType.Network:
                    model = LoadNetwork(model);
                    return PartialView("NetworkAlertModal", model);
                case AlertType.Hdd:
                    model = LoadHdd(model);
                    return PartialView("DriveAlertModal", model);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private AlertRules LoadNetwork(AlertRules rule)
        {
            var nics = HardwareService.GetAllNics();
            rule.NicDict = new Dictionary<string, int>();
            var ddlNics = new List<string>();
            foreach (var nic in nics)
            {
                rule.NicDict.Add(nic.Key, nic.Value);
                ddlNics.Add(nic.Key);
            }
            rule.Nics = new SelectList(rule.NicDict, "Value", "Key", 1);

            return rule;
        }

        private AlertRules LoadHdd(AlertRules rule)
        {
            var drives = HardwareService.GetDrives();

            rule.DrivesDict = new Dictionary<int, string>();
            var ddlDrives = new List<string>();
            foreach (var drive in drives.Where(drive => drive.IsReady))
            {
                rule.DrivesDict.Add(drive.DriveId, drive.VolumeLabel);
                ddlDrives.Add(drive.VolumeLabel);
            }
            rule.Nics = new SelectList(rule.DrivesDict, "DriveId", "DriveVolumeLabel", 1);

            return rule;
        }
    }
}