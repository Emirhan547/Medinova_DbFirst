using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Dtos.Defaults
{
    public class DefaultAboutDto
    {
        public About About { get; set; }
        public IList<AboutItem> Items { get; set; }
    }
}