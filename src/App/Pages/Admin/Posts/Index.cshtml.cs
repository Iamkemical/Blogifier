﻿using Core.Data;
using Core.Helpers;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace App.Pages.Admin.Posts
{
    public class IndexModel : AdminPageModel
    {
        [BindProperty]
        public IEnumerable<PostItem> Posts { get; set; }

        public Pager Pager { get; set; }

        IUnitOfWork _db;
        ISearchService _ss;

        public IndexModel(IUnitOfWork db, ISearchService ss)
        {
            _db = db;
            _ss = ss;
            Pager = new Pager(1);
        }

        public async Task<IActionResult> OnGetAsync(int page = 1, string status = "A")
        {
            var author = await _db.Authors.GetItem(a => a.AppUserName == User.Identity.Name);
            IsAdmin = author.IsAdmin;

            Expression<Func<BlogPost, bool>> predicate = p => p.Id > 0;
            Pager = new Pager(page);

            if (IsAdmin)
            {
                if(status == "P")
                    predicate = p => p.Published > DateTime.MinValue;
                else if(status == "D")
                    predicate = p => p.Published == DateTime.MinValue;
            }
            else
            {
                if (status == "P")
                    predicate = p => p.Published > DateTime.MinValue && p.AuthorId == author.Id;
                else if (status == "D")
                    predicate = p => p.Published == DateTime.MinValue && p.AuthorId == author.Id;
                else
                    predicate = p => p.AuthorId == author.Id;
            }

            Posts = await _db.BlogPosts.Find(predicate, Pager);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var author = await _db.Authors.GetItem(a => a.AppUserName == User.Identity.Name);
            IsAdmin = author.IsAdmin;

            var page = int.Parse(Request.Form["page"]);
            var term = Request.Form["search"];

            Pager = new Pager(page);

            if(IsAdmin)
                Posts = await _ss.Find(Pager, term);
            else
                Posts = await _ss.Find(Pager, term, author.Id);

            return Page();
        }
    }
}