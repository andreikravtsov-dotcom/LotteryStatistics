/******************************************************************************
 *
 * File: Program.cs
 *
 * Description: A console application for retrieving lottery draw statistics for Bingo, Viking Lotto, and Eurojackpot from Eesti Loto website.
 *
 * Date: 30.05.2026		Author: Andrei Kravtsov
 *
 *****************************************************************************/
using LotteryStatistics.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LotteryStatistics
{
    internal class Program
    {
        #region Member variables

        private enum LotteryType
        {
            BINGO,
            VIKINGLOTTO,
            EURO
        }

        private static string _csrfToken = string.Empty;
        private static string _savePath = string.Empty;
        private static LotteryType _lotteryType = LotteryType.BINGO;

        #endregion /Member variables

        static async Task Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("LotteryStatistics.exe <SavePath> <LotteryName>");
                    return;
                }

                _savePath = args[0];
                string lotteryName = args[1];

                switch (lotteryName.ToLower())
                {
                    case "bingo":
                        _lotteryType = LotteryType.BINGO;
                        await UpdateDraws();
                        break;
                    case "viking":
                        _lotteryType = LotteryType.VIKINGLOTTO;
                        await UpdateDraws();
                        break;
                    case "eurojackpot":
                        _lotteryType = LotteryType.EURO;
                        await UpdateDraws();
                        break;
                    default:
                        Console.WriteLine("Invalid lottery name. Please specify 'Bingo or Viking or Eurojackpot'.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        #region Private Methods

        /// <summary>
        /// Updates the draws by fetching new results and saving them to a JSON file.
        /// </summary>
        /// <returns></returns>
        private static async Task UpdateDraws()
        {
            try
            {
                //statistics files does not exist
                if (!System.IO.File.Exists(Path.Combine(_savePath, $"{Enum.GetName(_lotteryType)}draws.json")))
                {
                    Console.WriteLine($"Creating new {Enum.GetName(_lotteryType)} draws file...");

                    var allResults = await GetAllDraws();
                    await SaveJsonFile(allResults);
                }
                //we need to add new draws to the existing file
                else
                {
                    Console.WriteLine($"Updating existing {Enum.GetName(_lotteryType)} draws file...");

                    List<LotteryDraw> oldDraws = await ReadDrawsFromFile();
                    LotteryDraw lastDraw = oldDraws.OrderByDescending(d => d.DrawDate).First();
                    var newDraws = await GetAllDraws();

                    await AddNewDraws(lastDraw, oldDraws, newDraws);
                    await SaveJsonFile(oldDraws);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Reads the existing draws from the JSON file.
        /// </summary>
        /// <returns></returns>
        private static async Task <List<LotteryDraw>> ReadDrawsFromFile()
        {
            try
            {
                string json = await System.IO.File.ReadAllTextAsync(Path.Combine(_savePath, $"{Enum.GetName(_lotteryType)}draws.json"));

                switch (_lotteryType)
                {
                    case LotteryType.BINGO:
                        return JsonSerializer.Deserialize<List<BingoDraw>>(json)!
                            .Cast<LotteryDraw>()
                            .ToList();
                    case LotteryType.EURO:
                        return JsonSerializer.Deserialize<List<EurojackpotDraw>>(json)!
                            .Cast<LotteryDraw>()
                            .ToList();
                    case LotteryType.VIKINGLOTTO:
                        return JsonSerializer.Deserialize<List<VikinglottoDraw>>(json)!
                            .Cast<LotteryDraw>()
                            .ToList();
                    default:
                        throw new NotSupportedException($"Unsupported lottery type {_lotteryType}");
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Saves the bingo results to a JSON file.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private static async Task SaveJsonFile(List<LotteryDraw> results)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json;

                if (results.All(x => x is BingoDraw))
                {
                    json = JsonSerializer.Serialize(
                        results.Cast<BingoDraw>().ToList(),
                        options);
                }
                else if (results.All(x => x is EurojackpotDraw))
                {
                    json = JsonSerializer.Serialize(
                        results.Cast<EurojackpotDraw>().ToList(),
                        options);
                }
                else if (results.All(x => x is VikinglottoDraw))
                {
                    json = JsonSerializer.Serialize(
                        results.Cast<VikinglottoDraw>().ToList(),
                        options);
                }
                else
                {
                    throw new NotSupportedException("Unknown or mixed lottery draw types.");
                }

                var filePath = Path.Combine(_savePath, $"{Enum.GetName(_lotteryType)}draws.json");
                await System.IO.File.WriteAllTextAsync(filePath, json);

                Console.WriteLine($"{Enum.GetName(_lotteryType)} results saved to {filePath}");
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Adds new bingo draws to the existing list of results.
        /// </summary>
        /// <param name="lastDraw"></param>
        /// <param name="oldResults"></param>
        /// <param name="newResults"></param>
        /// <returns></returns>
        private static async Task AddNewDraws(LotteryDraw lastDraw, List<LotteryDraw> oldResults, List<LotteryDraw> newResults)
        {
            foreach (var draw in newResults)
            {
                if (draw.DrawDate > lastDraw.DrawDate)
                {
                    oldResults.Add(draw);
                    Console.WriteLine($"New draw added: {draw.DrawLabel}");
                }
            }
        }

        /// <summary>
        /// Gets all draws from the Eesti Loto website.
        /// </summary>
        /// <returns></returns>
        private static async Task<List<LotteryDraw>> GetAllDraws()
        {
            try
            {
                Console.WriteLine($"Fetching ALL {Enum.GetName(_lotteryType)} results from Eesti Loto website...");

                List<LotteryDraw> allResults = new List<LotteryDraw>();
                int pageIndex = 1;
                string orderBy = "drawDate_asc";

                while (true)
                {
                    var results = await GetDraws(pageIndex, orderBy);
                    if (results == null || results.Count == 0)
                        break;
                    allResults.AddRange(results);
                    pageIndex++;
                }
                return allResults;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets draws from the Eesti Loto website for a specific page index and order.
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        private static async Task<List<LotteryDraw>?> GetDraws(int pageIndex, string orderBy)
        {
            try
            {
                using HttpClient http = await GetHttpClientAsync();

                var form = new Dictionary<string, string>
                {
                    ["gameTypes"] = Enum.GetName(_lotteryType)!.ToUpper(),
                    ["dateFrom"] = "",
                    ["dateTo"] = "",
                    ["drawLabelFrom"] = "",
                    ["drawLabelTo"] = "",
                    ["pageIndex"] = pageIndex.ToString(),
                    ["orderBy"] = orderBy,
                    ["sortLabelNumeric"] = "true",
                    ["csrfToken"] = _csrfToken
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, "https://www.eestiloto.ee/app/ajaxDrawStatistic");

                request.Headers.Referrer = new Uri("https://www.eestiloto.ee/et/results/");
                request.Headers.Add("Origin", "https://www.eestiloto.ee");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                request.Content = new FormUrlEncodedContent(form);
                request.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/x-www-form-urlencoded")
                    {
                        CharSet = "UTF-8"
                    };

                var response = await http.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                var dto = JsonSerializer.Deserialize<EestiLotoResponse>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return ConvertToLotteryDraws(dto!, pageIndex);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Converts the Eesti Loto response DTO to a list of LotteryDraw objects based on the lottery type.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        private static List<LotteryDraw>? ConvertToLotteryDraws(EestiLotoResponse dto, int pageIndex)
        {
            try 
            {
                if (dto != null && dto.Draws != null)
                {
                    Console.WriteLine($"Fetched {dto.Draws.Count} {Enum.GetName(_lotteryType)} draws from page {pageIndex}");

                    switch (_lotteryType)
                    {
                        case LotteryType.BINGO:
                            {
                                var bingoDraws = dto!.Draws.Select(draw =>
                                {
                                    var numbersText = draw.Results
                                        .FirstOrDefault(r => r.WinClass == 2)
                                        ?.WinningNumber;

                                    return (LotteryDraw)new BingoDraw
                                    {
                                        DrawId = draw.DrawId,
                                        DrawLabel = int.Parse(draw.DrawLabel),
                                        DrawDate = DateTimeOffset
                                            .FromUnixTimeMilliseconds(draw.DrawDate)
                                            .LocalDateTime,

                                        Numbers = numbersText!
                                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(x => int.Parse(x.Trim()))
                                            .ToList()
                                    };
                                }).ToList();
                                return bingoDraws;
                            }
                        case LotteryType.VIKINGLOTTO:
                            {
                                var vikingDraws = dto!.Draws.Select(draw =>
                                {
                                    var result = draw.Results.First();

                                    return (LotteryDraw)new VikinglottoDraw
                                    {
                                        DrawId = draw.DrawId,
                                        DrawLabel = int.Parse(draw.DrawLabel),
                                        DrawDate = DateTimeOffset
                                            .FromUnixTimeMilliseconds(draw.DrawDate)
                                            .LocalDateTime,

                                        MainNumbers = result.WinningNumber!
                                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(x => int.Parse(x.Trim()))
                                            .ToList(),

                                        VikingNumber = int.Parse(result.SecWinningNumber!)
                                    };
                                }).ToList();
                                return vikingDraws;
                            }

                        case LotteryType.EURO:
                            {
                                var euroDraws = dto!.Draws.Select(draw =>
                                {
                                    var result = draw.Results.FirstOrDefault();

                                    return (LotteryDraw)new EurojackpotDraw
                                    {
                                        DrawId = draw.DrawId,
                                        DrawLabel = int.Parse(draw.DrawLabel),
                                        DrawDate = DateTimeOffset
                                        .FromUnixTimeMilliseconds(draw.DrawDate)
                                        .LocalDateTime,

                                        MainNumbers = result!.WinningNumber!
                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => int.Parse(x.Trim()))
                                        .ToList(),

                                        EuroNumbers = result.SecWinningNumber!
                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => int.Parse(x.Trim()))
                                        .ToList()
                                    };
                                }).ToList();
                                return euroDraws;
                            }
                        default:
                            throw new NotSupportedException($"Lottery type {_lotteryType} is not supported.");
                    }
                   
                }
                else
                    return null;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets the CSRF token and cookies from the Eesti Loto website.
        /// </summary>
        /// <returns></returns>
        private static async Task<HttpClient> GetHttpClientAsync()
        {
            try
            {
                CookieContainer cookies = new();

                var handler = new HttpClientHandler
                {
                    CookieContainer = cookies,
                    AutomaticDecompression = DecompressionMethods.GZip |
                                            DecompressionMethods.Deflate |
                                            DecompressionMethods.Brotli
                };

                var http = new HttpClient(handler);

                http.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/149 Safari/537.36");

                http.DefaultRequestHeaders.Accept.ParseAdd("*/*");
                http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9,ru;q=0.8");

                var resultsPage = await http.GetStringAsync("https://www.eestiloto.ee/et/results/");
                var csrfMatch = Regex.Match(resultsPage, @"[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}");

                if (!csrfMatch.Success)
                    throw new Exception("CSRF token not found");

                _csrfToken = csrfMatch.Value;
                return http;
            }
            catch
            {
                throw;
            }
        }

        #endregion /Private Methods
    }
}
