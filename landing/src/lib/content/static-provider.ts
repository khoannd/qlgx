import type {
  Article,
  ArticleCategory,
  ArticleSummary,
  ContentProvider,
  LandingContent,
  Release,
} from "./types";

/**
 * Nguồn nội dung tĩnh — dữ liệu nằm ngay trong mã nguồn.
 *
 * Dùng cho giai đoạn chưa có CMS. Khi có CMS thật, viết một lớp khác cũng
 * thoả `ContentProvider` rồi đổi trong `provider.ts`; giao diện không đổi gì.
 */

/**
 * Nút tải luôn trỏ vào endpoint của chính trang, không trỏ thẳng tới tệp.
 * Endpoint mới là nơi quyết định phiên bản nào là mới nhất rồi chuyển hướng sang
 * GitHub Releases — xem src/app/api/tai-ve/[kenh]/route.ts.
 */
const DOWNLOAD_ENDPOINT = "/api/tai-ve";

// Nội dung "release note" hiển thị công khai CỐ Ý giữ nguyên theo bản 4.0.0 —
// 4.0.1 và 4.0.2 chỉ là các bản vá kỹ thuật cho chính đợt phát hành 4.0.0 (lỗi
// bộ cài thiếu thư mục, rồi lỗi cài đè không thật sự cập nhật chương trình),
// chưa ai từng cài hai bản đó nên không cần giải thích cho người dùng cuối.
// Tệp tải và số phiên bản hiển thị vẫn phải là 4.0.2 — đó là bản cài THẬT SỰ
// hoạt động đúng. Nội dung chi tiết từng bản vẫn lưu đầy đủ, trung thực ở
// src/lib/version-history.ts (trang /phien-ban) cho ai muốn xem kỹ.
const release: Release = {
  version: "4.0.2",
  publishedAt: "2026-09-07",
  headline: "Nền tảng mới, chạy khoẻ trên Windows 10 và 11",
  summary:
    "Bản nâng cấp lớn nhất từ trước tới nay: chương trình được chuyển sang nền tảng .NET Framework 4.8 và làm lại cách kết nối với Microsoft Office đời mới.",
  groups: [
    {
      id: "nen-tang",
      title: "Nâng cấp nền tảng",
      icon: "download",
      items: [
        "Chuyển từ .NET Framework 2.0 lên 4.8 để chạy ổn định trên Windows 10 và Windows 11.",
        "Làm lại cách kết nối với Microsoft Word và Excel cho phù hợp các phiên bản Office mới.",
        "Cập nhật lại toàn bộ các mẫu in.",
      ],
    },
    {
      id: "office",
      title: "Sửa lỗi xuất Word & Excel",
      icon: "check-square",
      items: [
        "Chương trình tự nhận ra lỗi kết nối Office và mời sửa ngay tại chỗ báo lỗi.",
        "Chỉ xoá thông tin thừa của bản Office cũ, giữ nguyên bản Office đang dùng — an toàn cho máy dùng Office 2010 và 2013.",
        "Tự sao lưu registry ra tệp trước khi sửa, để có thể khôi phục lại.",
      ],
    },
    {
      id: "mgc",
      title: "Nhập dữ liệu từ phần mềm MGC",
      icon: "database",
      items: [
        "Nhập đầy đủ cả những người chỉ có tên trong Sổ Rửa tội mà chưa có phần Lý lịch.",
        "Giữ nguyên lý lịch đã có: ngày qua đời, số điện thoại, dân tộc, địa chỉ.",
        "Tự kiểm tra và cài giúp Microsoft Access Database Engine khi máy còn thiếu, có xác thực chữ ký số của Microsoft.",
      ],
    },
    {
      id: "bi-tich",
      title: "Sổ bí tích & gia đình",
      icon: "book",
      items: [
        "Sửa lỗi mục Hôn phối trong Sổ bí tích chỉ hiện mã số thay vì tên người chồng và người vợ.",
        "Khi chọn vợ hoặc chồng mà người còn lại đã qua đời, chương trình không tự đưa người đó vào gia đình nữa.",
      ],
    },
  ],
  // Chỉ MỘT bộ cài công khai. Bộ cài đặt (.msi qua GXInstaller.vdproj) đã tự xử
  // lý cả hai trường hợp: máy chưa có QLGX thì cài mới, máy đã có bản cũ thì
  // nâng cấp tại chỗ — không cần gỡ trước. Đây là hành vi "Major Upgrade" chuẩn
  // của Windows Installer (UpgradeCode cố định, RemovePreviousVersions=TRUE,
  // DetectNewerInstalledVersion=TRUE — xem Source/GXInstaller/GXInstaller.vdproj).
  //
  // Có một endpoint khác `/api/tai-ve/update` trỏ tới gói .zip, nhưng KHÔNG hiện
  // ở đây: gói đó chỉ dành cho chính AutoUpdate.exe của chương trình tự tải khi
  // kiểm tra cập nhật — không phải một trình cài đặt, người dùng tự tải về rồi
  // tự chạy sẽ không biết làm gì với nó.
  downloads: [
    {
      id: "full",
      tag: "Cài mới & cập nhật",
      title: "Bộ cài đặt QLGX 4.0.2",
      summary:
        "Dùng được cho cả máy cài lần đầu lẫn máy đang chạy bản cũ. Nếu máy đã có QLGX, cứ chạy thẳng bộ cài này — nó tự nhận ra và nâng cấp tại chỗ, không cần gỡ bản cũ trước, dữ liệu giáo dân giữ nguyên.",
      fileName: "qlgx_4_0_2.exe",
      size: "Khoảng 8,2 MB",
      href: `${DOWNLOAD_ENDPOINT}/full`,
      primary: true,
      cta: "Tải bộ cài QLGX 4.0.2",
    },
  ],
  articleSlug: "phat-hanh-phien-ban-4-0-0",
};

/* ------------------------------------------------------------------ */
/* Nội dung trang chủ — sửa được qua /admin/trang-chu                  */
/* ------------------------------------------------------------------ */

const landingContent: LandingContent = {
  trustStats: [
    { value: "Miễn phí", label: "Không giới hạn số giáo dân" },
    { value: "Win 7 → 11", label: "Chạy trên máy tính sẵn có" },
    { value: "Ngoại tuyến", label: "Không cần Internet để dùng" },
    { value: "Tiếng Việt", label: "Thuật ngữ Công giáo chuẩn" },
  ],

  painPoints: [
    {
      title: "Trích lục mất cả buổi chiều",
      body: "Tìm một chứng nhận Rửa tội phải lật từng trang qua nhiều cuốn sổ, có khi vẫn không tìm ra.",
    },
    {
      title: "Giấy thì cũ đi, mực thì phai",
      body: "Sổ bộ ẩm mốc và hư hỏng theo thời gian. Mất một cuốn là mất vĩnh viễn ký ức của cả một giai đoạn.",
    },
    {
      title: "Thống kê cuối năm đếm tay",
      body: "Số liệu gửi giáo phận phải đếm thủ công từng người, dễ sai và không đối chiếu lại được.",
    },
    {
      title: "Hồ sơ hôn phối viết lại từ đầu",
      body: "Rao hôn phối, điều tra hôn phối, giấy giới thiệu — mỗi lần một bộ, chép tay lại toàn bộ thông tin.",
    },
  ],

  features: [
    {
      id: "giao-dan",
      icon: "users",
      tone: "cobalt",
      title: "Hồ sơ giáo dân",
      body: "Lý lịch đầy đủ: tên thánh, ngày sinh, giáo họ, trình độ, nghề nghiệp, ảnh chân dung và toàn bộ các bí tích đã lãnh nhận trên cùng một màn hình.",
    },
    {
      id: "gia-dinh",
      icon: "home",
      tone: "ruby",
      title: "Gia đình & giáo họ",
      body: "Lập phiếu gia đình, xác định chủ hộ, vai trò từng thành viên; chuyển hộ giữa các giáo họ hoặc chuyển xứ mà không sai lệch dữ liệu.",
    },
    {
      id: "so-bi-tich",
      icon: "book",
      tone: "amber",
      title: "Sổ bí tích",
      body: "Rửa tội, Thêm sức, Xưng tội — Rước lễ lần đầu, Hôn phối và Qua đời. Đánh số bí tích theo từng sổ, nhập theo đợt cho cả lớp, cả nhóm.",
    },
    {
      id: "hon-phoi",
      icon: "heart",
      tone: "emerald",
      title: "Rao & điều tra hôn phối",
      body: "Lập danh sách rao hôn phối theo ngày Chúa nhật, in tờ rao, tờ điều tra hôn phối và giấy giới thiệu theo mẫu sẵn có.",
    },
    {
      id: "giao-ly",
      icon: "graduation",
      tone: "ruby",
      title: "Giáo lý & hội đoàn",
      body: "Quản lý lớp giáo lý theo niên khoá, danh sách học viên kèm tên cha mẹ; theo dõi lịch sử tham gia hội đoàn của từng giáo dân.",
    },
    {
      id: "thong-ke",
      icon: "chart",
      tone: "cobalt",
      title: "Thống kê & báo cáo",
      body: "Thống kê sinh, rửa tội, thêm sức, hôn phối, qua đời, tân tòng theo năm và theo giáo họ. Kết xuất ra Excel để gửi giáo phận.",
    },
    {
      id: "in-an",
      icon: "printer",
      tone: "emerald",
      title: "In ấn trên Word & Excel",
      body: "Phiếu gia đình, lý lịch cá nhân, chứng nhận Rửa tội — Hôn phối, giấy giới thiệu… In hàng loạt, mẫu in có thể tự chỉnh sửa.",
    },
    {
      id: "sao-luu",
      icon: "refresh",
      tone: "amber",
      title: "Sao lưu & khôi phục",
      body: "Sao lưu toàn bộ dữ liệu ra một tệp nén chỉ với một cú nhấp. Nhập dữ liệu sẵn có từ phần mềm MGC hoặc từ Excel.",
    },
    {
      id: "phan-quyen",
      icon: "shield",
      tone: "cobalt",
      title: "Phân quyền người dùng",
      body: "Tạo tài khoản riêng cho từng người — Người quản trị, Người nhập liệu — với quyền hạn khác nhau. Dữ liệu nhạy cảm của giáo dân được bảo vệ.",
    },
  ],

  installSteps: [
    "Tải bộ cài đặt về máy rồi bấm đúp để chạy. Nếu Windows hỏi, chọn “Vẫn chạy” (Run anyway). Máy đã có QLGX bản cũ thì cứ chạy thẳng — không cần gỡ trước.",
    "Bấm Tiếp tục qua các bước, giữ nguyên thư mục cài đặt được đề nghị.",
    "Nếu là máy cài lần đầu: mở chương trình, khai báo thông tin giáo xứ và các giáo họ.",
    "Bắt đầu nhập giáo dân, hoặc dùng chức năng nhập từ Excel / từ phần mềm MGC nếu đã có dữ liệu sẵn. (Máy nâng cấp từ bản cũ thì dữ liệu đã có sẵn, bỏ qua bước 3 và 4.)",
  ],

  // Hằng số riêng, không import từ site.ts: static-provider.ts thuộc lớp nguồn
  // dữ liệu, còn site.ts thuộc lớp thiết kế trang — tránh phụ thuộc ngược.
  helpLinks: [
    {
      id: "cai-dat",
      icon: "book",
      title: "Hướng dẫn cài đặt",
      subtitle: "Cài lần đầu và cập nhật từ bản cũ",
      href: "/huong-dan/cai-dat",
      external: false,
    },
    {
      id: "nhap-lieu",
      icon: "pencil",
      title: "Cách nhập liệu",
      subtitle: "Bắt đầu nhập giáo dân, gia đình và sổ bí tích",
      href: "/huong-dan/cach-nhap-lieu",
      external: false,
    },
    {
      id: "hoi-dap",
      icon: "help",
      title: "Hỏi — đáp cùng tác giả",
      subtitle: "Báo lỗi và đề nghị tính năng mới",
      href: "https://forum.quanlygiaoxu.net",
      external: true,
    },
    // Giá trị literal, không import SUPPORT_EMAIL/FACEBOOK_URL từ site.ts —
    // static-provider.ts thuộc lớp nguồn dữ liệu, site.ts thuộc lớp thiết kế
    // trang, tránh phụ thuộc ngược (xem chú thích tương tự ở helpLinks phía trên).
    {
      id: "email",
      icon: "mail",
      title: "Email hỗ trợ",
      subtitle: "hotro@quanlygiaoxu.net",
      href: "mailto:hotro@quanlygiaoxu.net",
      external: false,
    },
    {
      id: "facebook",
      icon: "facebook",
      title: "Trang Facebook QLGX",
      subtitle: "Cập nhật tin tức và thông báo mới nhất",
      href: "https://www.facebook.com/qlgx2013",
      external: true,
    },
  ],

  faqs: [
    {
      id: "mien-phi",
      question: "Phần mềm Quản Lý Giáo Xứ có mất phí không?",
      answer: [
        "Không. Phần mềm được cung cấp miễn phí cho các giáo xứ, không giới hạn số lượng giáo dân, gia đình hay số máy cài đặt.",
      ],
    },
    {
      id: "du-lieu",
      question: "Dữ liệu giáo dân được lưu ở đâu?",
      answer: [
        "Toàn bộ dữ liệu nằm trong một tệp cơ sở dữ liệu ngay trên máy tính của giáo xứ, không gửi lên bất kỳ máy chủ nào. Giáo xứ hoàn toàn làm chủ dữ liệu của mình.",
        "Vì vậy, hãy sao lưu định kỳ ra USB hoặc ổ đĩa ngoài bằng chức năng Sao lưu dữ liệu có sẵn trong chương trình.",
      ],
    },
    {
      id: "cau-hinh",
      question: "Máy tính cần cấu hình gì để chạy được phần mềm?",
      answer: [
        "Một máy tính Windows thông thường là đủ: Windows 7, 8, 10 hoặc 11, và .NET Framework 4.8 (Windows 10 và 11 đã có sẵn).",
        "Để in phiếu gia đình và các giấy chứng nhận, máy cần cài Microsoft Word hoặc Microsoft Excel.",
      ],
    },
    {
      id: "cap-nhat",
      question: "Giáo xứ đang dùng bản cũ, cập nhật lên bản mới nhất thế nào?",
      answer: [
        "Tải và chạy thẳng bộ cài đặt mới nhất — không cần gỡ bản cũ trước. Bộ cài tự nhận ra máy đã có QLGX và nâng cấp tại chỗ, giống hệt cách cài lần đầu.",
        "Dữ liệu giáo dân nằm trong một tệp cơ sở dữ liệu riêng, tách biệt hoàn toàn khỏi chương trình, nên việc cài đè không đụng tới. Dù vậy, hãy luôn sao lưu trước khi cập nhật — đó là thói quen an toàn.",
      ],
    },
    {
      id: "mgc",
      question: "Đang dùng phần mềm MGC, có chuyển dữ liệu sang được không?",
      answer: [
        "Được. Chương trình có chức năng nhập dữ liệu trực tiếp từ phần mềm MGC, và bản 4.0.0 đã nhập được đầy đủ hơn hẳn các bản trước — kể cả những người chỉ có tên trong Sổ Rửa tội. Ngoài ra còn có chức năng nhập từ tệp Excel theo mẫu có sẵn.",
      ],
    },
    {
      id: "nhieu-nguoi",
      question: "Nhiều người cùng tham gia nhập liệu thì sao?",
      answer: [
        "Chương trình cho phép tạo nhiều tài khoản với quyền hạn khác nhau: \"Người quản trị\" xem và sửa được mọi thứ, \"Người nhập liệu\" chỉ nhập và xem dữ liệu. Mỗi người đăng nhập bằng tài khoản riêng, tuỳ giáo xứ sắp xếp ai giữ vai trò nào.",
      ],
    },
    {
      id: "ho-tro",
      question: "Gặp lỗi hoặc cần thêm tính năng thì liên hệ ở đâu?",
      answer: [
        "Xin đăng câu hỏi trên diễn đàn forum.quanlygiaoxu.net. Tác giả và các giáo xứ khác sẽ cùng hỗ trợ. Chương trình cũng có sẵn mục Gởi tin nhắn cho tác giả ngay trong menu.",
      ],
    },
  ],
};

/* ------------------------------------------------------------------ */
/* Chuyên mục                                                          */
/* ------------------------------------------------------------------ */

const categories: Record<string, ArticleCategory> = {
  "phat-hanh": { slug: "phat-hanh", name: "Phát hành", tone: "ruby" },
  "huong-dan": { slug: "huong-dan", name: "Hướng dẫn", tone: "cobalt" },
  "gioi-thieu": { slug: "gioi-thieu", name: "Giới thiệu", tone: "emerald" },
  "thong-bao": { slug: "thong-bao", name: "Thông báo", tone: "amber" },
};

/* ------------------------------------------------------------------ */
/* Bài viết mẫu                                                        */
/* ------------------------------------------------------------------ */

const articles: Article[] = [
  {
    slug: "phat-hanh-phien-ban-4-0-0",
    title: "Phát hành phiên bản 4.0.0",
    excerpt:
      "Chương trình chuyển sang nền tảng .NET Framework 4.8, làm lại cách kết nối Microsoft Office và sửa hàng loạt lỗi khi nhập dữ liệu từ phần mềm MGC.",
    category: categories["phat-hanh"],
    publishedAt: "2026-09-04",
    readingMinutes: 4,
    author: "Ban phát triển",
    body: [
      {
        type: "paragraph",
        text: "Sau hơn hai năm kể từ bản 3.3.7, phiên bản 4.0.0 đã sẵn sàng để các giáo xứ tải về. Đây là bản nâng cấp lớn nhất từ trước tới nay, tập trung vào việc giúp chương trình chạy ổn định trên những máy tính đời mới.",
      },
      { type: "heading", text: "Vì sao phải nâng cấp nền tảng" },
      {
        type: "paragraph",
        text: "Các bản trước được xây trên .NET Framework 2.0 — một nền tảng ra đời từ năm 2005. Windows 10 và Windows 11 không còn cài sẵn nền tảng này, nên nhiều giáo xứ phải tự tìm cách cài thêm trước khi dùng được chương trình. Từ bản 4.0.0, chương trình chạy trên .NET Framework 4.8, vốn có sẵn trong mọi bản Windows 10 và 11.",
      },
      { type: "heading", text: "Sửa lỗi không xuất được Word và Excel" },
      {
        type: "paragraph",
        // Bỏ chữ "nhiều nhất": chưa có số liệu thống kê thật để khẳng định đây
        // là lỗi được báo nhiều hơn các lỗi khác — chỉ biết chắc là có nhiều
        // giáo xứ từng gặp, không biết chắc là "nhiều nhất".
        text: "Đây là lỗi nhiều giáo xứ từng gặp. Nguyên nhân nằm ở thông tin còn sót lại của một bản Microsoft Office cũ đã gỡ khỏi máy. Chương trình nay tự nhận ra lỗi này và mời sửa ngay tại chỗ báo lỗi, thay vì bắt người dùng tự tìm vào menu.",
      },
      {
        type: "list",
        items: [
          "Chức năng sửa chỉ xoá thông tin thừa của bản Office cũ, giữ nguyên bản Office đang dùng.",
          "Bản trước có thể xoá nhầm, gây hỏng trên máy dùng Office 2010 hoặc Office 2013 — bản này đã khắc phục.",
          "Registry được sao lưu ra tệp trước khi sửa, để có thể khôi phục nếu cần.",
          "Sửa được cho cả Microsoft Word lẫn Microsoft Excel.",
        ],
      },
      { type: "heading", text: "Nhập dữ liệu từ phần mềm MGC" },
      {
        type: "paragraph",
        text: "Trước đây, nếu số người trong phần Lý lịch ít hơn số người trong Sổ Rửa tội thì chương trình bỏ qua toàn bộ những người chỉ có tên trong Sổ Rửa tội. Nay chương trình nhập đầy đủ tất cả, đồng thời giữ nguyên lý lịch đã có sẵn: ngày qua đời, số điện thoại, dân tộc và địa chỉ.",
      },
      {
        type: "note",
        text: "Trước khi cập nhật, hãy vào menu Hệ thống › Sao lưu dữ liệu và cất tệp sao lưu ra USB hoặc ổ đĩa ngoài. Cài đè bộ cài mới lên bản cũ không đụng tới dữ liệu (dữ liệu nằm trong một tệp cơ sở dữ liệu riêng), nhưng sao lưu vẫn luôn là thói quen nên có.",
      },
    ],
  },
  {
    slug: "chuyen-du-lieu-tu-so-giay-sang-phan-mem",
    title: "Chuyển sổ bộ giấy sang phần mềm: bắt đầu từ đâu",
    excerpt:
      "Một giáo xứ vài nghìn nhân danh không thể nhập xong trong một tuần. Đây là thứ tự công việc giúp việc số hoá không bị dở dang giữa chừng.",
    category: categories["huong-dan"],
    publishedAt: "2026-08-18",
    readingMinutes: 6,
    author: "Ban phát triển",
    body: [
      {
        type: "paragraph",
        // Bỏ chữ "nhiều nhất": không có số liệu đếm câu hỏi trên diễn đàn để
        // khẳng định đây là câu được hỏi nhiều hơn mọi câu khác.
        text: "Một câu hỏi thường gặp trên diễn đàn không phải là cách dùng một chức năng cụ thể, mà là: nên bắt đầu từ đâu. Kinh nghiệm từ các giáo xứ đã làm xong cho thấy thứ tự công việc quan trọng hơn tốc độ nhập liệu.",
      },
      { type: "heading", text: "Bước 1 — Khai báo khung trước, nhập người sau" },
      {
        type: "paragraph",
        text: "Khai báo thông tin giáo xứ, danh sách giáo họ và danh sách các cha coi sóc trước tiên. Những thông tin này sẽ tự động in ra trên mọi giấy chứng nhận sau này. Nếu để sau, sẽ phải sửa lại hàng loạt hồ sơ đã nhập.",
      },
      { type: "heading", text: "Bước 2 — Nhập theo giáo họ, không nhập theo sổ" },
      {
        type: "paragraph",
        text: "Nhiều giáo xứ bắt đầu bằng cách nhập lần lượt từ Sổ Rửa tội. Cách này khiến dữ liệu bị rời rạc: một người có tên trong sổ nhưng chưa có gia đình, chưa có giáo họ. Nên nhập trọn từng giáo họ một — hết gia đình này sang gia đình khác — rồi mới bổ sung số bí tích.",
      },
      {
        type: "list",
        items: [
          "Nhập người chồng và người vợ trước, sau đó thêm các con vào gia đình.",
          "Nhập ngày và số bí tích ngay khi nhập người, đừng để dồn lại làm sau.",
          "Người đã qua đời hoặc đã chuyển xứ vẫn nhập, rồi đánh dấu tình trạng — đừng bỏ qua.",
        ],
      },
      { type: "heading", text: "Bước 3 — Kiểm tra dữ liệu định kỳ" },
      {
        type: "paragraph",
        text: "Chương trình có sẵn hai chức năng Kiểm tra dữ liệu giáo dân và Kiểm tra dữ liệu gia đình. Chạy chúng sau mỗi giáo họ, đừng đợi tới cuối. Sửa mười lỗi ngay lúc còn nhớ dễ hơn nhiều so với sửa năm trăm lỗi sau sáu tháng.",
      },
      {
        type: "note",
        text: "Nếu giáo xứ đã có dữ liệu trong phần mềm MGC hoặc trong một tệp Excel, hãy dùng chức năng nhập sẵn có thay vì gõ tay lại từ đầu.",
      },
    ],
  },
  {
    slug: "sao-luu-du-lieu-dung-cach",
    title: "Sao lưu dữ liệu đúng cách",
    excerpt:
      "Dữ liệu nằm trên máy của giáo xứ, nghĩa là giáo xứ chịu trách nhiệm giữ gìn nó. Ba nguyên tắc để không bao giờ mất trắng công sức nhiều năm.",
    category: categories["huong-dan"],
    publishedAt: "2026-07-30",
    readingMinutes: 3,
    author: "Ban phát triển",
    body: [
      {
        type: "paragraph",
        text: "Phần mềm không gửi dữ liệu lên bất kỳ máy chủ nào — toàn bộ hồ sơ giáo dân nằm trong một tệp trên máy tính của giáo xứ. Đó là điều tốt cho quyền riêng tư, nhưng cũng có nghĩa là không ai sao lưu giúp cả.",
      },
      { type: "heading", text: "Ba nguyên tắc" },
      {
        type: "list",
        items: [
          "Sao lưu hằng tuần, không phải hằng năm. Đặt một ngày cố định, ví dụ chiều thứ Bảy sau khi nhập liệu xong.",
          "Giữ tệp sao lưu ở nơi khác với máy tính. USB cất trong tủ phòng thánh, hoặc ổ đĩa ngoài — cháy hay mất trộm thì máy và bản sao không cùng mất.",
          "Thỉnh thoảng thử khôi phục. Một tệp sao lưu chưa từng được thử khôi phục thì chưa chắc dùng được.",
        ],
      },
      { type: "heading", text: "Cách làm" },
      {
        type: "paragraph",
        text: "Vào menu Hệ thống › Sao lưu dữ liệu, chọn nơi lưu rồi bấm đồng ý. Chương trình nén toàn bộ dữ liệu vào một tệp duy nhất. Tệp này mang sang máy khác vẫn khôi phục được nguyên vẹn.",
      },
    ],
  },
  {
    slug: "loi-ngo-tu-tac-gia",
    title: "Lời ngỏ từ tác giả",
    excerpt:
      "Phần mềm ra đời từ một nhu cầu có thật trong giáo xứ, và lớn lên nhờ chính những góp ý của người dùng suốt nhiều năm — trong đó có cả các quý Cha.",
    category: categories["gioi-thieu"],
    // Ngày đăng THẬT, lấy từ mốc thời gian của chính bài trên phpBB
    // ("Thứ 6, 17 Tháng 6, 2011") — đã xác minh lại, không phải suy đoán.
    // Trước đó ghi nhầm "2020-06-18": không có căn cứ, khả năng cao là nhớ lẫn
    // với ngày sửa đổi (18/06/2020) của các tệp trong BIN/help/.
    // Lưu ý: bài này được tác giả cập nhật dần theo thời gian (nội dung nhắc
    // tới sự kiện tháng 9/2012, sau ngày đăng gốc) nên "ngày đăng" ở đây chỉ
    // là mốc khởi tạo bài viết, không phải ngày viết ra câu chữ cuối cùng.
    publishedAt: "2011-06-17",
    readingMinutes: 4,
    author: "Mátthêu Nguyễn Đức Khoan",
    body: [
      {
        type: "paragraph",
        text: "Số lượng giáo dân của các giáo xứ ngày càng tăng. Việc quản lý thông tin giáo dân trên giấy tờ sổ sách đã trở thành một việc hết sức khó khăn. Nhiều phần mềm quản lý giáo dân đã có, nhưng hầu hết đều vướng vấn đề khó khăn khi cài đặt và thao tác sử dụng khá phức tạp — trong khi không phải ai trong giáo xứ cũng thành thạo vi tính. Yêu cầu đặt ra là có một phần mềm đơn giản hơn, thân thiện hơn, dễ cài đặt hơn.",
      },
      {
        type: "paragraph",
        text: "Là một giáo dân được ơn Chúa cho chút khả năng về tin học, cùng với sự gợi ý của một người bạn đáng kính, tôi đã gấp rút hoàn thành phần mềm quản lý giáo xứ — đầu tiên là chức năng quản lý giáo dân. Ứng dụng những kỹ thuật học được trong quá trình làm việc tại một công ty phần mềm, tôi hy vọng phần mềm này đem lại hiệu quả thiết thực, như một món quà nhỏ dành tặng cho giáo hội Việt Nam.",
      },
      {
        type: "quote",
        text: "Con luôn tâm niệm một điều rằng, tất cả là do Chúa làm, con chỉ là công cụ.",
      },
      {
        type: "paragraph",
        text: "Sau lần ra mắt phiên bản đầu tiên, tôi liên tục nhận được ý kiến phản hồi từ khắp nơi gửi về. Đến nay, chương trình đã được dùng rộng rãi khắp các giáo phận trên mọi miền đất nước, kể cả một vài giáo xứ Việt ở nước ngoài — trong đó có sự trợ giúp và góp ý quý báu của Cha Giuse Nguyễn Văn Soi, Giáo phận Phan Thiết. Có thể nói QLGX là sản phẩm của chính những người trực tiếp sử dụng chương trình.",
      },
      { type: "heading", text: "Đôi nét về người viết phần mềm" },
      {
        type: "list",
        items: [
          "Mátthêu Nguyễn Đức Khoan — giáo dân giáo xứ Tân Hội, Phan Rang, Ninh Thuận.",
          "Hiện sinh hoạt tại giáo xứ Tân Đức, Quận 9, TP. Hồ Chí Minh.",
          "Phát triển phần mềm tại công ty Primas.",
          "Giảng viên bộ môn Kỹ thuật phần mềm, Trường Đại học Sư phạm Kỹ thuật TP. Hồ Chí Minh.",
        ],
      },
      {
        type: "note",
        text: "Trích và biên tập lại từ mục Giới thiệu phần mềm trên diễn đàn forum.quanlygiaoxu.net, do chính tác giả đăng và cập nhật dần qua nhiều năm.",
      },
    ],
  },
  {
    slug: "duoc-nhieu-giao-phan-chon-dung-chung",
    title: "Được nhiều giáo phận chọn dùng thống nhất",
    excerpt:
      "Từ một phần mềm cho một giáo xứ, QLGX lần lượt được các Đấng Bản quyền cho phép sử dụng thống nhất trong toàn giáo phận.",
    category: categories["thong-bao"],
    // Cùng nguồn và cùng lý do lấy ngày như bài "Lời ngỏ từ tác giả" ở trên —
    // xem ghi chú tại đó.
    publishedAt: "2011-06-17",
    readingMinutes: 3,
    author: "Ban phát triển",
    body: [
      {
        type: "paragraph",
        text: "Trong quá trình phát triển, chương trình đã được một số giáo phận chọn dùng thống nhất, theo đúng những gì tác giả từng chia sẻ trên diễn đàn:",
      },
      {
        type: "list",
        items: [
          "Tháng 1/2010 — Giáo phận Phan Thiết: chương trình được giới thiệu đến tất cả các giáo xứ trong giáo phận để dùng chung, dưới sự đồng ý của Đức Cha Giuse Võ Duy Thống.",
          "Ngày 09/03/2011 — Giáo phận Vinh: trong Thư mục vụ Mùa Chay 2011, Đức Giám mục Phaolô Nguyễn Thái Hợp chính thức ấn định QLGX là phần mềm thống nhất chung cho toàn giáo phận.",
          "Tháng 5/2011 — Giáo phận Phú Cường: Cha đại diện truyền thông giáo phận liên lạc và triển khai nhập dữ liệu cho toàn giáo phận.",
          "Tháng 9/2012 — Giáo phận Qui Nhơn: Đức Giám mục Mátthêu cho phép sử dụng phần mềm chung toàn giáo phận.",
        ],
      },
      {
        type: "note",
        text: "Thông tin trên tổng hợp từ mục Giới thiệu phần mềm trên diễn đàn forum.quanlygiaoxu.net. Nếu giáo phận của quý vị cũng đang dùng chung QLGX mà chưa có trong danh sách, xin liên hệ qua diễn đàn để cập nhật.",
      },
    ],
  },
  {
    slug: "chuan-bi-thong-ke-cuoi-nam",
    title: "Chuẩn bị số liệu thống kê cuối năm cho giáo phận",
    excerpt:
      "Danh sách những việc nên làm trong tháng Mười Một để tới cuối năm chỉ còn việc bấm nút xuất báo cáo.",
    category: categories["thong-bao"],
    publishedAt: "2026-05-20",
    readingMinutes: 3,
    author: "Ban phát triển",
    body: [
      {
        type: "paragraph",
        text: "Mỗi cuối năm, các giáo xứ đều phải gửi số liệu về giáo phận: số rửa tội, thêm sức, hôn phối, qua đời và tân tòng trong năm. Nếu dữ liệu đã được nhập đều đặn suốt năm, việc này chỉ mất vài phút.",
      },
      {
        type: "list",
        items: [
          "Rà lại các bí tích đã cử hành trong năm, đối chiếu với sổ giấy xem đã nhập đủ chưa.",
          "Đánh dấu những người đã qua đời trong năm — đây là mục hay bị bỏ sót nhất.",
          "Chạy Kiểm tra dữ liệu giáo dân để phát hiện hồ sơ thiếu ngày tháng.",
          "Vào Thống kê chung, chọn khoảng năm, bấm Thống kê rồi xuất kết quả ra Excel.",
        ],
      },
      {
        type: "note",
        text: "Kết quả thống kê xuất ra là tệp Excel thông thường, có thể sửa lại cho khớp với mẫu báo cáo riêng của từng giáo phận.",
      },
    ],
  },
];

/* ------------------------------------------------------------------ */

const toSummary = ({ body: _body, ...summary }: Article): ArticleSummary => summary;

const byNewest = (a: ArticleSummary, b: ArticleSummary) =>
  b.publishedAt.localeCompare(a.publishedAt);

export const staticContentProvider: ContentProvider = {
  async getRelease() {
    return release;
  },

  async listArticles(options) {
    let result = articles.map(toSummary).sort(byNewest);
    if (options?.category) {
      result = result.filter((a) => a.category.slug === options.category);
    }
    if (options?.limit) {
      result = result.slice(0, options.limit);
    }
    return result;
  },

  async getArticle(slug) {
    return articles.find((a) => a.slug === slug) ?? null;
  },

  async listArticleSlugs() {
    return articles.map((a) => a.slug);
  },

  async listCategories() {
    return Object.values(categories);
  },

  async getLandingContent() {
    return landingContent;
  },
};
