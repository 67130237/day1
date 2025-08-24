using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1/cms")]
public class CmsController : ControllerBase
{
    [HttpGet("banners")]
    public ActionResult<IEnumerable<BannerItem>> Banners()
        => Ok(new[] { new BannerItem("b1", "โปรพิเศษ", "https://img.example.com/b1.png", "/apply") });

    [HttpGet("articles")]
    public ActionResult<IEnumerable<ArticleItem>> Articles()
        => Ok(new[] { new ArticleItem("loan-basics", "พื้นฐานสินเชื่อ", "เนื้อหา...") });

    [HttpGet("articles/{slug}")]
    public ActionResult<ArticleItem> Article(string slug)
        => Ok(new ArticleItem(slug, "พื้นฐานสินเชื่อ", "เนื้อหา..."));
}