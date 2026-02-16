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
[Route("admin/telefon-inceleme")]
public sealed class TelefonIncelemeController : Controller
{
    private const int AdminPageSize = 20;

    private readonly ITelefonReviewService _telefonReviewService;

    public TelefonIncelemeController(ITelefonReviewService telefonReviewService)
    {
        _telefonReviewService = telefonReviewService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] string? q, [FromQuery] int page = 1, CancellationToken ct = default)
    {
        var safePage = Math.Max(page, 1);
        var (items, totalCount) = await _telefonReviewService.GetAdminPagedAsync(q, safePage, AdminPageSize, ct);
        var paging = PagingHelper.Normalize(safePage, AdminPageSize, totalCount);

        return View(new TelefonReviewAdminListViewModel
        {
            Items = items,
            Query = q,
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = totalCount,
            TotalPages = paging.TotalPages
        });
    }

    [HttpGet("yeni")]
    public async Task<IActionResult> Create([FromQuery] string? slug, CancellationToken ct)
    {
        var input = new TelefonReviewEditorInputModel
        {
            TelefonSlug = slug?.Trim().ToLowerInvariant() ?? string.Empty
        };

        return View("Editor", await BuildEditorViewModelAsync(input, isEditMode: false, ct));
    }

    [HttpGet("duzenle/{slug}")]
    public async Task<IActionResult> Edit(string slug, CancellationToken ct)
    {
        var review = await _telefonReviewService.GetByTelefonSlugAsync(slug, ct);
        if (review is null)
        {
            return RedirectToAction(nameof(Create), new { slug });
        }

        var input = new TelefonReviewEditorInputModel
        {
            TelefonSlug = review.TelefonSlug,
            Title = review.Title,
            Excerpt = review.Excerpt,
            RawContent = review.RawContent,
            SeoTitle = review.SeoTitle,
            SeoDescription = review.SeoDescription
        };

        return View("Editor", await BuildEditorViewModelAsync(input, isEditMode: true, ct));
    }

    [HttpPost("kaydet")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(TelefonReviewEditorInputModel input, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var invalidViewModel = await BuildEditorViewModelAsync(input, isEditMode: !string.IsNullOrWhiteSpace(input.TelefonSlug), ct);
            return View("Editor", invalidViewModel);
        }

        var result = await _telefonReviewService.UpsertAsync(input.TelefonSlug, ToUpsertInput(input), ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Inceleme kaydedilemedi.");
            var failedViewModel = await BuildEditorViewModelAsync(input, isEditMode: !string.IsNullOrWhiteSpace(input.TelefonSlug), ct);
            return View("Editor", failedViewModel);
        }

        return RedirectToAction(nameof(Edit), new { slug = result.Review!.TelefonSlug });
    }

    [HttpPost("sil/{slug}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string slug, CancellationToken ct)
    {
        await _telefonReviewService.DeleteAsync(slug, ct);
        return RedirectToAction(nameof(Index));
    }

    private static TelefonReviewUpsertInput ToUpsertInput(TelefonReviewEditorInputModel input)
    {
        return new TelefonReviewUpsertInput(
            Title: input.Title,
            Excerpt: input.Excerpt,
            RawContent: input.RawContent,
            SeoTitle: input.SeoTitle,
            SeoDescription: input.SeoDescription);
    }

    private async Task<TelefonReviewAdminEditorViewModel> BuildEditorViewModelAsync(
        TelefonReviewEditorInputModel input,
        bool isEditMode,
        CancellationToken ct)
    {
        var suggestions = await _telefonReviewService.GetAdminPhoneSuggestionsAsync(200, ct);
        return new TelefonReviewAdminEditorViewModel
        {
            PageTitle = isEditMode ? "Telefon Inceleme Duzenle" : "Telefon Inceleme Olustur",
            SubmitLabel = isEditMode ? "Guncelle" : "Kaydet",
            IsEditMode = isEditMode,
            Input = input,
            PhoneSuggestions = suggestions
        };
    }
}
