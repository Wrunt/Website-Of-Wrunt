using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Configuration;
using System.IO;
using System.Collections;

namespace WruntsTools.Models
{
    public class MapAffixModel
    {
        const int UnidentifiedBonus = 30;
        public List<Affix> AffixLst { get; set; }
        public List<Affix> IncludedPrefixLst { get; set; }
        public List<Affix> IncludedSuffixLst { get; set; }
        public ItemRarity Rarity { get { return _rarity; } set { _rarity = value; UpdateMods(); } }
        private ItemRarity _rarity { get; set; }
        public int? Quality { get; set; }
        public int? Bonuses { get; set; }
        public int? Quantity { get; set; }
        private int? BaseQuantityBonuses { get { return this.Quality + this.Bonuses + UnidentifiedBonus; } }
        public int? RemainingQuantity { get; set; }
        public Mods NumberOfMods { get; set; }

        public List<Affix> PossibleAffixes { get; set; }

        public MapAffixModel()
        {
            XElement xml = XElement.Load(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/MapAffixes.xml"));

            this.AffixLst = new List<Affix>();
            this.IncludedPrefixLst = new List<Affix>();
            this.IncludedSuffixLst = new List<Affix>();
            this.PossibleAffixes = new List<Affix>();
            this.NumberOfMods = new Mods();

            this.Quality = null;
            this.Bonuses = null;
            this.Quantity = null;
            this.RemainingQuantity = null;

            this.AffixLst = xml.Descendants().Where(x => x.Name.LocalName == "Affix")
                               .Select(a => new Affix() { Type = a.Element("Type").Value.Trim(), Name = a.Element("Name").Value.Trim(), 
                                PropertyLst = a.Elements("Property").Select(x => x.Value.Trim()).ToList(), Quantity = Int32.Parse(a.Element("Quantity").Value.Trim()) }).ToList();
        }

        const int MaxQuality = 20;
        public SelectList GetQualityList()
        {
            return new SelectList(Enumerable.Range(0, MaxQuality));
        }

        public SelectList GetRarityList()
        {
            return new SelectList(Enum.GetValues(typeof(ItemRarity)));
        }

        public bool HasAffix(string name)
        {
            return (this.IncludedPrefixLst.Any(a => a.Name == name) || this.IncludedSuffixLst.Any(a => a.Name == name));
        }

        public void UpdateRemainingQuantity()
        {
            this.RemainingQuantity = this.Quantity - (this.Quality + this.Bonuses + UnidentifiedBonus);
            if (this.IncludedPrefixLst.Count > 0)
            {
                this.RemainingQuantity -= this.IncludedPrefixLst.Select(ipl => ipl.Quantity).Sum();
            }
            if (this.IncludedSuffixLst.Count > 0)
            {
                this.RemainingQuantity -= this.IncludedSuffixLst.Select(isl => isl.Quantity).Sum();
            }
            UpdateMods();
        }

        public void UpdateMods()
        {
            int min = this.Rarity == ItemRarity.Magic ? 0 : 1;
            int max = this.Rarity == ItemRarity.Magic ? 1 : 3;

            this.NumberOfMods.Prefix.Min = min;
            this.NumberOfMods.Suffix.Min = min;

            this.NumberOfMods.Prefix.Current = IncludedPrefixLst.Count;
            this.NumberOfMods.Suffix.Current = IncludedSuffixLst.Count;

            this.NumberOfMods.Prefix.Max = max;
            this.NumberOfMods.Suffix.Max = max;

            SetPossibleAffixes();
        }

        public void SetPossibleAffixes()
        {
            List<Affix> possiblePrefixes = new List<Affix>();

            possiblePrefixes = AffixLst.Where(a => a.Type == "Prefix" && !IncludedPrefixLst.Contains(a) && a.Quantity <= this.RemainingQuantity).ToList();

            List<Affix> possibleSuffixes = new List<Affix>();

            possibleSuffixes = AffixLst.Where(a => a.Type == "Suffix" && !IncludedSuffixLst.Contains(a) && a.Quantity <= this.RemainingQuantity).ToList();

            possiblePrefixes = this.NumberOfMods.Prefix.Current < this.NumberOfMods.Prefix.Max ? possiblePrefixes : new List<Affix>();
            possibleSuffixes = this.NumberOfMods.Suffix.Current < this.NumberOfMods.Suffix.Max ? possibleSuffixes : new List<Affix>();

            pph = new List<Affix>();
            pph = possiblePrefixes;
            psh = new List<Affix>();
            psh = possibleSuffixes;

            possiblePrefixes = possiblePrefixes.Where(p => p.Quantity == 0 || IsAffixPossible(p)).ToList();
            possibleSuffixes = possibleSuffixes.Where(p => p.Quantity == 0 || IsAffixPossible(p)).ToList();

            this.PossibleAffixes = new List<Affix>();
            this.PossibleAffixes.AddRange(possiblePrefixes);
            this.PossibleAffixes.AddRange(possibleSuffixes);
        }

        private List<Affix> pph { get; set; }
        private List<Affix> psh { get; set; }

        public bool IsAffixPossible(Affix aff)
        {
            bool affIsPrefix = aff.Type.ToLower() == "prefix";
            string curr = affIsPrefix ? "IncludedPrefixLst" : "IncludedSuffixLst";
            string comp = affIsPrefix ? "IncludedSuffixLst" : "IncludedPrefixLst";
            
            List<Affix> baseList = new List<Affix>();
            baseList = (List<Affix>)this.GetType().GetProperty(curr).GetValue(this);
            List<Affix> compList = new List<Affix>();
            compList = (List<Affix>)this.GetType().GetProperty(comp).GetValue(this);

            int prefixCount = affIsPrefix ? this.NumberOfMods.Prefix.Current + 1 : this.NumberOfMods.Prefix.Current;
            int suffixCount = affIsPrefix ? this.NumberOfMods.Suffix.Current : this.NumberOfMods.Suffix.Current + 1;

            int quantity = (baseList.Count > 0 ? baseList.Select(p => p.Quantity).Sum() : 0) + (compList.Count > 0 ? compList.Select(p => p.Quantity).Sum() : 0);

            if ((quantity + aff.Quantity + this.BaseQuantityBonuses > this.Quantity) ||
                (prefixCount > this.NumberOfMods.Prefix.Max || suffixCount > this.NumberOfMods.Prefix.Max))
            {
                return false;
            }

            int qr = this.Quantity.GetValueOrDefault() - (quantity + aff.Quantity + this.BaseQuantityBonuses.GetValueOrDefault());
            
            if (qr < 0)
            {
                return false;
            }
            else if (qr == 0 && (!affIsPrefix ? prefixCount >= this.NumberOfMods.Prefix.Min : suffixCount >= this.NumberOfMods.Suffix.Min))
            {
                return true;
            } else if (qr == 0 && (!affIsPrefix ? prefixCount < this.NumberOfMods.Prefix.Min : suffixCount < this.NumberOfMods.Suffix.Min))
            {
                return false;
            }

            if (this.Rarity == ItemRarity.Magic)
            {
                if (affIsPrefix && psh.Any(a => qr - a.Quantity == 0))
                {
                    return true;
                }
                else if (pph.Any(a => qr - a.Quantity == 0))
                {
                    return true;
                }
                return false;
            }

            return true;
        }
    }

    public class Affix
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public List<string> PropertyLst { get; set; }
        
        public int Quantity { get; set; }

        public Affix()
        {
            this.PropertyLst = new List<string>();
        }
    }

    public class Mods
    {
        public ModRange Prefix { get; set; }
        public ModRange Suffix { get; set; }
        
        public Mods()
        {
            this.Prefix = new ModRange();
            this.Suffix = new ModRange();
        }
    }

    public class ModRange
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int Current { get; set; }
    }
}