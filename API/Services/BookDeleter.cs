using DataTier.Entities.Abstract;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Services
{
    [DisallowConcurrentExecution]
    public class BookDeleter : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using (EFDbContext _context = new EFDbContext())
                {
                    var books = _context.Books.Include(b => b.Table);
                    foreach (var book in books)
                    {
                        TimeSpan ts = DateTime.Now - book.BookDate;
                        if ( ts.TotalMinutes >= 30 && book.Table.Status == TableStatus.Booked)
                        {
                            book.Table.Status = TableStatus.Free;
                            _context.Remove(book);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw new Exception("Возникло исключение");
            }
        }
    }
}
