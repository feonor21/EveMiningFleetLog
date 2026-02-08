using System.Collections.Generic;
using EveMiningFleet.Entities.Tables;

namespace EveMiningFleet.Models.ViewModel
{
    public class WhatIShouldMineModel
    {
        public decimal ReprocessingEfficiency { get; set; } = 85;
        public string rapport { get; set; } = "";

        public bool ConversionLancer { get; set; } = false;
        public bool ConversionOK { get; set; } = false;

        public Dictionary<string, Ore> AllOreModels = new Dictionary<string, Ore>();
        public List<string> Allorename = new List<string>();
    }

}
