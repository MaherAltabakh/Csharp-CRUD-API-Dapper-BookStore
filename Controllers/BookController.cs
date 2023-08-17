using BookStore.Models;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Formats.Asn1;
using System.Globalization;
using System.Net.Http.Json;

namespace BookStore.Controllers
{
    public class BookController : Controller
    {
        public IActionResult CreateAndUpdateBook(Book book)
        {
            ViewBag.errorMessage = TempData["errorMessage"];

            if ((ViewBag.errorMessage == null) && (book.BookID != 0))
            {
                HttpClient client = new HttpClient();
                var responseTask = client.GetAsync("https://localhost:7068/api/BookAPI/"+book.BookID);
                responseTask.Wait();
                if (responseTask.IsCompleted)
                {
                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var MessageTask = result.Content.ReadAsStringAsync();
                        var str = MessageTask.Result;
                        var bookToUpdate = JsonConvert.DeserializeObject<Book>(MessageTask.Result);

                        ViewBag.Book = bookToUpdate;
                    }
                }
            }
            else
                ViewBag.Book = book;

            return View();           
        }

        public IActionResult SaveCreateAndUpdateBook(Book book)
        {
            List<string> errorMessages = new List<string>();
            BookDB bookDb = new BookDB();

            if (book.Title == null)
                errorMessages.Add("Title is required");
            if (book.NumberofPages == 0)
                errorMessages.Add("Number of pages is required");


            if (errorMessages.Count > 0)
            {
                TempData["errorMessage"] = errorMessages;
                //here we are redirecting to the above action and pass the mistaken object to send it back to the HTML page to be edited
                return RedirectToAction("CreateAndUpdateBook", book);
            }

            if (book.BookID == 0)
            {
                book.CreatedDate = DateTime.Now;
                //call the API create action
                HttpClient client = new HttpClient();
                var responseTask = client.PostAsJsonAsync("https://localhost:7068/api/BookAPI/", book);
                responseTask.Wait();
            }
            else
            {
                book.UpdatedDate = DateTime.Now;
                //call the API update action
                HttpClient client = new HttpClient();
                var responseTask = client.PutAsJsonAsync("https://localhost:7068/api/BookAPI/"+book.BookID, book);
                responseTask.Wait();
            }
            
            return RedirectToAction("ReadBooks");
        }





         public IActionResult ReadBooks()
        {            
            HttpClient client = new HttpClient();
            var responseTask = client.GetAsync("https://localhost:7068/api/BookAPI");
            responseTask.Wait();
            if (responseTask.IsCompleted)
            {
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var MessageTask = result.Content.ReadAsStringAsync();
                    var str = MessageTask.Result;
                    var books = JsonConvert.DeserializeObject<List<Book>>(MessageTask.Result);

                    ViewBag.Book = books;
                }
            }
            return View();
        }

        public IActionResult DeleteBook(int BookID)
        {
            HttpClient client = new HttpClient();
            var responseTask = client.DeleteAsync("https://localhost:7068/api/BookAPI/"+BookID);
            responseTask.Wait();
         
            return RedirectToAction("ReadBooks");
        }
    }
}
