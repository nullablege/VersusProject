using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Kiyaslasana.BL.Helpers;
using Kiyaslasana.PL.Areas.Admin.Models;
using Kiyaslasana.PL.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiyaslasana.PL.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/blog")]
public sealed class BlogController : Controller
{
    private const int AdminPageSize = 20;

    private readonly IBlogPostService _blogPostService;

    public BlogController(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] int page = 1, CancellationToken ct = default)
    {
        var safePage = Math.Max(page, 1);
        var (items, totalCount) = await _blogPostService.GetAdminPagedAsync(safePage, AdminPageSize, ct);
        var paging = PagingHelper.Normalize(safePage, AdminPageSize, totalCount);

        return View(new BlogAdminListViewModel
        {
            Items = items,
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = totalCount,
            TotalPages = paging.TotalPages
        });
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View("Editor", new BlogAdminEditorViewModel
        {
            Id = null,
            PageTitle = "Blog Yazisi Olustur",
            SubmitLabel = "Kaydet",
            Input = new BlogPostEditorInputModel
            {
                PublishedAt = DateTimeOffset.UtcNow
            }
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogPostEditorInputModel input, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View("Editor", BuildEditorViewModel(null, input));
        }

        var result = await _blogPostService.CreateAsync(ToUpsertInput(input), ct);
        if (!result.Success || result.Post is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Blog yazisi kaydedilemedi.");
            return View("Editor", BuildEditorViewModel(null, input));
        }

        return RedirectToAction(nameof(Edit), new { id = result.Post.Id });
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var post = await _blogPostService.GetAdminByIdAsync(id, ct);
        if (post is null)
        {
            return NotFound();
        }

        return View("Editor", BuildEditorViewModel(id, new BlogPostEditorInputModel
        {
            Title = post.Title,
            Excerpt = post.Excerpt,
            ContentRaw = post.ContentRaw,
            MetaTitle = post.MetaTitle,
            MetaDescription = post.MetaDescription,
            IsPublished = post.IsPublished,
            PublishedAt = post.PublishedAt
        }));
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BlogPostEditorInputModel input, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View("Editor", BuildEditorViewModel(id, input));
        }

        var result = await _blogPostService.UpdateAsync(id, ToUpsertInput(input), ct);
        if (!result.Success || result.Post is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Blog yazisi guncellenemedi.");
            return View("Editor", BuildEditorViewModel(id, input));
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    private static BlogPostUpsertInput ToUpsertInput(BlogPostEditorInputModel model)
    {
        return new BlogPostUpsertInput(
            Title: model.Title,
            Excerpt: model.Excerpt,
            ContentRaw: model.ContentRaw,
            MetaTitle: model.MetaTitle,
            MetaDescription: model.MetaDescription,
            IsPublished: model.IsPublished,
            PublishedAt: model.PublishedAt);
    }

    private static BlogAdminEditorViewModel BuildEditorViewModel(int? id, BlogPostEditorInputModel input)
    {
        return new BlogAdminEditorViewModel
        {
            Id = id,
            PageTitle = id.HasValue ? "Blog Yazisi Duzenle" : "Blog Yazisi Olustur",
            SubmitLabel = id.HasValue ? "Guncelle" : "Kaydet",
            Input = input
        };
    }
}
