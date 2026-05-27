using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

[Authorize(Roles = "Admin,Teacher")]
public class QuestionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public QuestionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int assessmentId)
    {
        var assessment = await _context.Assessments
            .Include(a => a.Course)
            .Include(a => a.Questions)
            .FirstOrDefaultAsync(a => a.AssessmentId == assessmentId);

        if (assessment == null)
        {
            return NotFound();
        }

        if (!await CanModifyAssessmentAsync(assessmentId))
        {
            return Forbid();
        }

        ViewBag.Assessment = assessment;
        return View(assessment.Questions.OrderBy(q => q.QuestionId).ToList());
    }

    public async Task<IActionResult> Create(int? assessmentId)
    {
        await LoadAssessmentsAsync(assessmentId);
        return View(new Question { AssessmentId = assessmentId ?? 0, Points = 10, CorrectAnswer = "A" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Question question)
    {
        NormalizeAnswer(question);

        if (!await CanModifyAssessmentAsync(question.AssessmentId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            await LoadAssessmentsAsync(question.AssessmentId);
            return View(question);
        }

        try
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Question added.";
            return RedirectToAction(nameof(Index), new { question.AssessmentId });
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "The question could not be saved. Please try again.");
            await LoadAssessmentsAsync(question.AssessmentId);
            return View(question);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var question = await _context.Questions.FindAsync(id);

        if (question == null)
        {
            return NotFound();
        }

        if (!await CanModifyAssessmentAsync(question.AssessmentId))
        {
            return Forbid();
        }

        await LoadAssessmentsAsync(question.AssessmentId);
        return View(question);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Question question)
    {
        if (id != question.QuestionId)
        {
            return BadRequest();
        }

        var existing = await _context.Questions.AsNoTracking().FirstOrDefaultAsync(q => q.QuestionId == id);

        if (existing == null)
        {
            return NotFound();
        }

        NormalizeAnswer(question);

        if (!await CanModifyAssessmentAsync(existing.AssessmentId) || !await CanModifyAssessmentAsync(question.AssessmentId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            await LoadAssessmentsAsync(question.AssessmentId);
            return View(question);
        }

        try
        {
            _context.Update(question);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Question updated.";
            return RedirectToAction(nameof(Index), new { question.AssessmentId });
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "The question could not be updated. Please try again.");
            await LoadAssessmentsAsync(question.AssessmentId);
            return View(question);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var question = await _context.Questions.Include(q => q.Assessment).FirstOrDefaultAsync(q => q.QuestionId == id);

        if (question == null)
        {
            return NotFound();
        }

        if (!await CanModifyAssessmentAsync(question.AssessmentId))
        {
            return Forbid();
        }

        return View(question);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var question = await _context.Questions.FindAsync(id);

        if (question == null)
        {
            return NotFound();
        }

        if (!await CanModifyAssessmentAsync(question.AssessmentId))
        {
            return Forbid();
        }

        try
        {
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Question deleted.";
        }
        catch (Exception)
        {
            TempData["Error"] = "The question could not be deleted because it has student answers.";
        }

        return RedirectToAction(nameof(Index), new { question.AssessmentId });
    }

    private static void NormalizeAnswer(Question question)
    {
        question.CorrectAnswer = (question.CorrectAnswer ?? string.Empty).Trim().ToUpperInvariant();
    }

    private async Task<bool> CanModifyAssessmentAsync(int assessmentId)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var teacherId = User.GetUserId();
        return await _context.Assessments.AnyAsync(a =>
            a.AssessmentId == assessmentId && a.Course != null && a.Course.TeacherId == teacherId);
    }

    private async Task LoadAssessmentsAsync(int? selectedAssessmentId = null)
    {
        var assessments = _context.Assessments.Include(a => a.Course).OrderBy(a => a.Title).AsQueryable();

        if (User.IsInRole("Teacher"))
        {
            var teacherId = User.GetUserId();
            assessments = assessments.Where(a => a.Course != null && a.Course.TeacherId == teacherId);
        }

        ViewBag.Assessments = new SelectList(await assessments.ToListAsync(), "AssessmentId", "Title", selectedAssessmentId);
        ViewBag.AnswerOptions = new SelectList(new[] { "A", "B", "C", "D" });
    }
}
