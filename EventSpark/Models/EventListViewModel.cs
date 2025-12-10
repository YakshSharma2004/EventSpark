using System;
using System.Collections.Generic;
using EventSpark.Core.Entities;

namespace EventSpark.Web.Models
{
    public class EventListViewModel
    {
        // Filters
        public string? SearchTerm { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string? SelectedCity { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSize);

        // Data
        public IEnumerable<Event> Events { get; set; } = new List<Event>();
        public IEnumerable<EventCategory> Categories { get; set; } = new List<EventCategory>();
        public IEnumerable<string> Cities { get; set; } = new List<string>();
    }
}
