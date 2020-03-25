﻿using HtmlAgilityPack;
using Navred.Core.Abstractions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Providers.Template
{
    public class Crawler : ICrawler
    {
        private readonly ILegRepository repo;

        public Crawler(ILegRepository repo)
        {
            this.repo = repo;
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var legs = await this.GetLegsAsync("URL");

                await this.repo.UpdateLegsAsync(legs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<IEnumerable<Leg>> GetLegsAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var legs = new List<Leg>();

            return legs;
        }
    }
}
