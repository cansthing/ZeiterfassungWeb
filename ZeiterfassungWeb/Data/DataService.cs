
using Microsoft.EntityFrameworkCore;
using ZeiterfassungWeb.Data.Models;
using System.Globalization;
using System;
using System.Runtime.Intrinsics.X86;

namespace ZeiterfassungWeb.Data
{
    public class DataService
    {
        private readonly ApplicationDbContext _context;
        public DataService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }
        
        public async Task<bool> CreateTimeBlock(TimeBlock timeBlock)
        {
            await _context.TimeBlocks.AddAsync(timeBlock);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateTimeBlock(TimeBlock timeBlock)
        {
            _context.TimeBlocks.Update(timeBlock);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteTimeBlock(int timeBlockId)
        {
            var timeBlock = await _context.TimeBlocks.FindAsync(timeBlockId);
            if (timeBlock == null)
            {
                return false;
            }
            _context.TimeBlocks.Remove(timeBlock);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<TimeBlock?> GetCurrentTimeBlock(int personalId)
        {
            TimeBlock? timeBlock = null;
            timeBlock = await _context.TimeBlocks.SingleOrDefaultAsync<TimeBlock>(t => t.PersonalId == personalId && t.End == null);
            return timeBlock;
        }
        public async Task<List<TimeBlock>> GetTimeBlocksOfToday(int personalId)
        {
            List<TimeBlock> timeBlocks = await _context.TimeBlocks.Where(t => t.PersonalId == personalId).ToListAsync();
            return timeBlocks.Where(d => d?.Start.Value.Date == DateTime.Today).ToList();
        }
        private async Task<List<TimeBlock>> GetAllTimeBlocks(int personalId)
        {
            List<TimeBlock> timeBlocks = await _context.TimeBlocks.Where(t => t.PersonalId == personalId && t.Start != null).ToListAsync();
            return timeBlocks;
        }
        public async Task<List<TimeBlock>> GetTimeBlocksDay(int personalId, DateTime date)
        {
            List<TimeBlock> timeBlocks = await GetAllTimeBlocks(personalId);
            timeBlocks = timeBlocks.Where(tb => ((DateTime)tb.Start).Date == date.Date).ToList();
            return timeBlocks;
        }
        public async Task<List<TimeBlock>> GetTimeBlocksWeek(int personalId, int kw, int year)
        {
            List<TimeBlock> timeBlocks = await GetAllTimeBlocks(personalId);
            timeBlocks = timeBlocks.Where(tb => ISOWeek.GetWeekOfYear((DateTime)tb.Start) == kw && ((DateTime)tb.Start).Year == year).ToList();
            return timeBlocks;
        }
        public async Task<List<TimeBlock>> GetTimeBlocksMonth(int personalId, int month, int year)
        {
            List<TimeBlock> timeBlocks = await GetAllTimeBlocks(personalId);
            timeBlocks = timeBlocks.Where(tb => ((DateTime)tb.Start).Month == month && ((DateTime)tb.Start).Year == year).ToList();
            return timeBlocks;
        }


        public async Task<DailyWork?> GetDailyWork(int personalId, DateTime? dateTime = null, DateOnly? date = null)
        {
            if (dateTime == null && date == null)
            {
                return null;
            }

            var targetDate = dateTime?.Date ?? Convert.ToDateTime(date).Date;
            List<TimeBlock> timeBlocks = await GetTimeBlocksDay(personalId, targetDate);

            TimeSpan totalWorkTime = TimeSpan.Zero;
            TimeSpan totalBreakTime = TimeSpan.Zero;

            foreach (var timeBlock in timeBlocks)
            {
                if (timeBlock.Start == null)
                    continue;
                if(timeBlock.End == null)
                {
                    timeBlock.End = DateTime.Now;
                }

                DateTime start = timeBlock.Start.Value;
                DateTime end = timeBlock.End.Value;

                TimeSpan
                    workTime = TimeSpan.Zero,
                    breakTime = TimeSpan.Zero;

                switch (timeBlock.IsWork)
                {
                    case true:
                        workTime = end - start;
                        break;
                    case false:
                        breakTime = end - start;
                        break;
                }
                totalWorkTime += workTime;
                totalBreakTime += breakTime;
            }

            //if (totalWorkTime == TimeSpan.Zero)
            //    return null;

            return new DailyWork(Convert.ToDateTime(targetDate), totalWorkTime, totalBreakTime);
        }
        public async Task<List<DailyWork>> GetDailyWorkWeek(int personalId, int kw, int year)
        {
            List<TimeBlock> timeBlocks = await GetTimeBlocksWeek(personalId, kw, year);
            List<DailyWork> dailyWorkList = new List<DailyWork>();
            foreach (TimeBlock timeBlock in timeBlocks)
            {
                if (timeBlock.Start == null || timeBlock.End == null)
                {
                    continue;
                }

                DateTime start = (DateTime)timeBlock.Start;
                DateTime end = (DateTime)timeBlock.End;
                TimeSpan 
                    workTime = TimeSpan.Zero, 
                    breakTime = TimeSpan.Zero;

                switch (timeBlock.IsWork)
                {
                    case true:
                        workTime = end - start;
                        break;
                    case false:
                        breakTime = end - start;
                        break;
                }
                DailyWork dailyWork = new DailyWork(start, workTime, breakTime);

                // Check if the date already exists in the list
                var existingDailyWork = dailyWorkList.FirstOrDefault(dw => dw.Date == dailyWork.Date);
                if (existingDailyWork != null)
                {
                    existingDailyWork.TotalWorkTime += dailyWork.TotalWorkTime;
                    existingDailyWork.TotalBreakTime += dailyWork.TotalBreakTime;
                }
                else
                {
                    dailyWorkList.Add(dailyWork);
                }
            }
            dailyWorkList = DailyWork.FillMissingDays(input: dailyWorkList, year: year, value: kw, interval: DailyWork.DateInterval.CalendarWeek);
            return dailyWorkList.OrderBy(dw=>dw.Date).ToList();
        }
        public async Task<List<DailyWork>> GetDailyWorkMonth(int personalId, int month, int year)
        {
            List<TimeBlock> timeBlocks = await GetTimeBlocksMonth(personalId, month, year);
            List<DailyWork> dailyWorkList = new();

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                if (timeBlock.Start == null || timeBlock.End == null)
                    continue;

                DateTime start = timeBlock.Start.Value;
                DateTime end = timeBlock.End.Value;

                TimeSpan workTime = TimeSpan.Zero;
                TimeSpan breakTime = TimeSpan.Zero;

                if (timeBlock.IsWork)
                    workTime = end - start;
                else
                    breakTime = end - start;

                DailyWork dailyWork = new DailyWork(start, workTime, breakTime);

                var existing = dailyWorkList.FirstOrDefault(dw => dw.Date == dailyWork.Date);
                if (existing != null)
                {
                    existing.TotalWorkTime += dailyWork.TotalWorkTime;
                    existing.TotalBreakTime += dailyWork.TotalBreakTime;
                }
                else
                {
                    dailyWorkList.Add(dailyWork);
                }
            }

            dailyWorkList = DailyWork.FillMissingDays(
                input: dailyWorkList,
                year: year,
                value: month,
                interval: DailyWork.DateInterval.Month
            );

            return dailyWorkList.OrderBy(dw => dw.Date).ToList();
        }
    }
}
