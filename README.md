"# The Sample ConvertMVC5ToCore31 for .NET Conf Taiwan 2019 Conference" 
# （演講主題）該準備從 .NET Framework 4.x 遷移至 .NET Core 3.0 了嗎？

## 原始碼內容為下方【移轉實例 Demo】中使用的 Visual Studio (擴充套件／VsPackage) 原始碼

大綱：
* 該做好準備嗎？
    * 	為什麼需要準備？
* 談微軟近年來的改變
* .NET Framework vs. .NET Core
* 漫談 .NET Core & ASP.NET Core
* 最近又出現了 .NET 5？什麼是 .NET 5？
* 原有 ASP.NET MVC 5 遷移至 ASP.NET Core 3.0 的挑戰
* 官方的 Migration 工具
* 觀念澄清
    * (1). 雖然部分目前部分底層難以移植，但是其實也不需要移植，只需要【創造】+【取代】即可。
* 移轉實例 Demo：
    * 現有程式碼分析(.NET 可攜性分析工具) .NET Portability Analyzer
    * 使用 PortApi CLI 分析現有程式碼
    * 使用 .NET API 分析器
	「API 分析器」是以 NuGet 套件 Microsoft.DotNet.Analyzers.Compatibility 的形式提供
	探索已被取代的 API
    * 使用架構分析器 .NET Framework Analyzer
	Microsoft.CodeQuality.Analyzers
	Microsoft.CodeAnalysis.Analyzers
	Microsoft.AspNetCore.Mvc.Analyzers
    * 使用自行開發 VS Extension & CLI 工具移轉 ASP.NET Web API to ASP.NET Core


## 議程簡述：
.NET Core 這個原先只是提供企業一個［跨平台選擇］ 與 一個和 .NET Framework 並行發展的運作平台，然而近年來來慢慢有些轉變，因為，微軟一開始發展 .NET Core 除了為了【跨平台】之外，原先確實想要將 .NET Framework 的內容慢慢移植到 .NET Core 之中，但是從 .NET Core 2.0 開始，微軟慢慢發現這樣做並不切實際，而慢慢轉向來去定義一個標準，於是 .NET Standard 出現了！並透過 .NET Standard 來統一所有平台的 BCL API。甚至在未來的 2020 年一統江湖的 .NET 5 即將誕生，但是這對企業而言，這接踵而來轉變無疑產生了新的問題與新的挑戰，該如何因應以迎接這些挑戰？我在這場 Session 中將告訴大家，並使用我自己撰寫的工具來進行簡單的移轉。

因為所有的 Developer 其實都在做同一件事，就是當你每發佈（Release）或修正(Fix)一個問題(Bug)時，你得要改三個地方(.NET Core/.NET Framework/Xamarin)的時候，慢慢地你就會想怎麼樣可以『只改一次』？怎麼樣可以讓程式碼由【三份】變成【一份】，於是 .NET 5 誕生了

## 課程 Slide 連結
https://www.slideshare.net/GelisWu/net-framework-4x-net-core-30
