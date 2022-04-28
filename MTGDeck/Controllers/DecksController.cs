using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using MTGDeck.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;

namespace MTGDeck.Controllers
{
    [Authorize]
  public class DecksController : Controller
  {
    private readonly MTGDeckContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DecksController(UserManager<ApplicationUser> userManager, MTGDeckContext db)
    {
      _userManager = userManager;
      _db = db;
    }
    public async Task<ActionResult> Index()
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId); //wrap in 'if' statement
      var userDecks = _db.Decks.Where(entry => entry.User.Id == currentUser.Id).ToList();
      return View(userDecks);
    }
    [HttpPost]
    public async Task<ActionResult> Create(Deck deck, int CardId)
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      deck.User = currentUser;
      _db.Decks.Add(deck);
      _db.SaveChanges();
      if (CardId != 0)
      {
          _db.CardDecks.Add(new CardDeck() { CardId = CardId, DeckId = deck.DeckId });
      }
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public ActionResult Details(int id)
    {
      var thisCard = _db.Cards
          .Include(card => card.JoinEntities)
          .ThenInclude(join => join.Deck)
          .FirstOrDefault(card => card.CardId == id);
      return View(thisCard);
    } 

    public ActionResult Edit(int id)
    {
      Deck deck = _db.Decks.FirstOrDefault(model => model.DeckId == id);
      return View(deck);
    }
    [HttpPost]
    public ActionResult Edit(Deck deck)
    {
      _db.Entry(deck).State = EntityState.Modified;
      _db.SaveChanges();
      return RedirectToAction("Details", new { id = deck.DeckId });
    }
    public ActionResult Delete(int id)
    {
      var thisDeck = _db.Decks.FirstOrDefault(deck => deck.DeckId == id);
      return View(thisDeck);
    }

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      var thisDeck = _db.Decks.FirstOrDefault(deck => deck.DeckId == id);
      _db.Decks.Remove(thisDeck);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }
  }
}