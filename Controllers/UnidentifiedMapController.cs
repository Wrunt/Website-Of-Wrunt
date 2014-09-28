using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using WruntsTools.Models;

namespace WruntsTools.Controllers
{
    public class UnidentifiedMapController : Controller
    {
        public ActionResult Index()
        {
            MapAffixModel mam = new MapAffixModel();

            Session["mam"] = mam;

            return View(mam);
        }

        [HttpPost]
        public ActionResult AddAffix(string affixName)
        {
            MapAffixModel mam = new MapAffixModel();

            mam = (MapAffixModel)Session["mam"];

            if (!mam.HasAffix(affixName))
            {
                Affix aff = mam.AffixLst.Where(a => a.Name == affixName).FirstOrDefault();

                if (aff.Type == "Prefix")
                {
                    mam.IncludedPrefixLst.Add(aff);
                }
                else
                {
                    mam.IncludedSuffixLst.Add(aff);
                }
            }

            mam.UpdateRemainingQuantity();

            Session["mam"] = mam;

            return Json(mam);
        }

        [HttpPost]
        public ActionResult RemoveAffix(string affixName)
        {
            MapAffixModel mam = new MapAffixModel();

            mam = (MapAffixModel)Session["mam"];

            if (mam.HasAffix(affixName))
            {
                Affix aff = mam.AffixLst.Where(a => a.Name == affixName).FirstOrDefault();

                if (aff.Type == "Prefix")
                {
                    mam.IncludedPrefixLst.Remove(aff);
                }
                else
                {
                    mam.IncludedSuffixLst.Remove(aff);
                }
            }

            mam.UpdateRemainingQuantity();

            Session["mam"] = mam;

            return Json(mam);
        }

        [HttpPost]
        public ActionResult UpdateProperty(string PropName, string Value)
        {
            MapAffixModel mam = new MapAffixModel();

            mam = (MapAffixModel)Session["mam"];

            switch (PropName)
            {
                case "Rarity":
                    mam.GetType().GetProperty(PropName).SetValue(mam, (Value == "Rare" ? ItemRarity.Rare : ItemRarity.Magic), null);
                    break;

                case "Quality": case "Bonuses": case "Quantity":
                    mam.GetType().GetProperty(PropName).SetValue(mam, Int32.Parse(Value), null);
                    break;
            }

            mam.UpdateRemainingQuantity();

            Session["mam"] = mam;

            return Json(mam);
        }

        [HttpGet]
        public ActionResult GetPossibleAffixes()
        {
            MapAffixModel mam = (MapAffixModel)Session["mam"];

            mam.UpdateRemainingQuantity();

            return Json(mam.PossibleAffixes, JsonRequestBehavior.AllowGet);
        }
    }
}