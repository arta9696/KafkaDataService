using Confluent.Kafka;
using KafkaDataService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading;

namespace KafkaDataService.Controllers
{
    [ApiController]
    public class DataflowController : ControllerBase
    {
        private readonly WebContext _context;
        private readonly ILogger<DataflowController> _logger;
        static ConsumerConfig config = new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "dotnet-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true
        };
        static CancellationTokenSource source = new CancellationTokenSource();

        public DataflowController(ILogger<DataflowController> logger, WebContext context)
        {
            _logger = logger;
            _context = context;
        }

        //[HttpPost, Route("[controller]/AddPostsKafka")]
        //public IActionResult AddPostsKafka()
        //{
        //    using (var consumer = new ConsumerBuilder<Ignore, string>(config).SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}")).Build())
        //    {
        //        consumer.Subscribe("Posts");

        //        try
        //        {
        //            while (true)
        //            {
        //                var result = consumer.Consume(source.Token);
        //                if (result == null) { continue; }
        //                if (result.IsPartitionEOF) { break; }
        //                _context.Add(JsonSerializer.Deserialize<Posts>(result.Message.Value));
        //                _context.SaveChanges();
        //            }
        //        }
        //        catch (OperationCanceledException) { }
        //        consumer.Close();
        //    }

        //    return Ok();
        //}
        //[HttpPost, Route("[controller]/AddCommentsKafka")]
        //public IActionResult AddCommentsKafka()
        //{
        //    using (var consumer = new ConsumerBuilder<Ignore, string>(config).SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}")).Build())
        //    {
        //        consumer.Subscribe("Comments");

        //        try
        //        {
        //            while (true)
        //            {
        //                var result = consumer.Consume(source.Token);
        //                if (result == null) { continue; }
        //                if (result.IsPartitionEOF) { break; }
        //                _context.Add(JsonSerializer.Deserialize<Comments>(result.Message.Value));
        //                _context.SaveChanges();
        //            }
        //        }
        //        catch (OperationCanceledException) { }
        //        consumer.Close();
        //    }

        //    return Ok();
        //}

        [HttpGet, Route("[controller]/GetPosts/{id?}")]
        public IActionResult GetPosts(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }
            var item = _context.Posts.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        [HttpGet, Route("[controller]/GetComments/{id?}")]
        public IActionResult GetComments(int? id)
        {
            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }
            var item = _context.Comments.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        [HttpGet, Route("[controller]/GetReport")]
        public IActionResult GetReport()
        {
            //Top posts by comments
            var mostCommentedPosts = _context.Posts
                .Join(
                    _context.Comments,
                    post => post.Id,
                    comment => comment.PostId,
                    (post, comment) => new { Post = post, Comment = comment }
                )
                .GroupBy(x => x.Post)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToArray();

            //Short named, short commented posts
            var shorties = _context.Posts
                .Join(
                    _context.Comments,
                    post => post.Id,
                    comment => comment.PostId,
                    (post, comment) => new { Post = post, Comment = comment }
                )
                .Where(x => x.Post.Body.Length <= 10 && x.Comment.Description.Length <= 10)
                .Select(x => x.Post)
                .Distinct()
                .ToArray();
            //Longest coments authors
            var longestComments = _context.Comments
                .OrderByDescending(comment => comment.Description.Length)
                .Select(comment => comment.Author)
                .Take(10)
                .ToList();

            var json = "[" + JsonSerializer.Serialize(mostCommentedPosts) + "," + JsonSerializer.Serialize(shorties) + "," + JsonSerializer.Serialize(longestComments) + "]";
            return Ok(json);
        }
    }
}