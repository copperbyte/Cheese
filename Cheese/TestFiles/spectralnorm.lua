


<!DOCTYPE html>
<html>
  <head prefix="og: http://ogp.me/ns# fb: http://ogp.me/ns/fb# githubog: http://ogp.me/ns/fb/githubog#">
    <meta charset='utf-8'>
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
        <title>shootout/spectralnorm.lua at master · chaoslawful/shootout · GitHub</title>
    <link rel="search" type="application/opensearchdescription+xml" href="/opensearch.xml" title="GitHub" />
    <link rel="fluid-icon" href="https://github.com/fluidicon.png" title="GitHub" />
    <link rel="apple-touch-icon" sizes="57x57" href="/apple-touch-icon-114.png" />
    <link rel="apple-touch-icon" sizes="114x114" href="/apple-touch-icon-114.png" />
    <link rel="apple-touch-icon" sizes="72x72" href="/apple-touch-icon-144.png" />
    <link rel="apple-touch-icon" sizes="144x144" href="/apple-touch-icon-144.png" />
    <link rel="logo" type="image/svg" href="https://github-media-downloads.s3.amazonaws.com/github-logo.svg" />
    <meta property="og:image" content="https://github.global.ssl.fastly.net/images/modules/logos_page/Octocat.png">
    <meta name="hostname" content="github-fe116-cp1-prd.iad.github.net">
    <meta name="ruby" content="ruby 2.0.0p247-github5 (2013-06-27) [x86_64-linux]">
    <link rel="assets" href="https://github.global.ssl.fastly.net/">
    <link rel="xhr-socket" href="/_sockets" />
    
    


    <meta name="msapplication-TileImage" content="/windows-tile.png" />
    <meta name="msapplication-TileColor" content="#ffffff" />
    <meta name="selected-link" value="repo_source" data-pjax-transient />
    <meta content="collector.githubapp.com" name="octolytics-host" /><meta content="github" name="octolytics-app-id" /><meta content="c7ea32a7-9df7-42cb-b9e6-bc98adc4d284" name="octolytics-dimension-request_id" />
    

    
    
    <link rel="icon" type="image/x-icon" href="/favicon.ico" />

    <meta content="authenticity_token" name="csrf-param" />
<meta content="HSi2fQqjoDZfhECM6jtQ/xpYLMxTryeXaHtZnp9UKCM=" name="csrf-token" />

    <link href="https://github.global.ssl.fastly.net/assets/github-cca7892e8d61459fbaad34e9ec1703fdfda2ca8e.css" media="all" rel="stylesheet" type="text/css" />
    <link href="https://github.global.ssl.fastly.net/assets/github2-4aac1f1fa7a38c2fbe63b6951d4f85decbe92c03.css" media="all" rel="stylesheet" type="text/css" />
    

    

      <script src="https://github.global.ssl.fastly.net/assets/frameworks-f86a2975a82dceee28e5afe598d1ebbfd7109d79.js" type="text/javascript"></script>
      <script src="https://github.global.ssl.fastly.net/assets/github-698f9407fdd6ef56b4fe426c9cff31651cae351b.js" type="text/javascript"></script>
      
      <meta http-equiv="x-pjax-version" content="6ab0fcc2ab3e2f04f492c35da610e17c">

        <link data-pjax-transient rel='permalink' href='/chaoslawful/shootout/blob/6b5df60d5cc5665196c25972f585d3a31bb77637/spectralnorm.lua'>
  <meta property="og:title" content="shootout"/>
  <meta property="og:type" content="githubog:gitrepository"/>
  <meta property="og:url" content="https://github.com/chaoslawful/shootout"/>
  <meta property="og:image" content="https://github.global.ssl.fastly.net/images/gravatars/gravatar-user-420.png"/>
  <meta property="og:site_name" content="GitHub"/>
  <meta property="og:description" content="shootout - Shootout test between lua/luajit/java/php"/>

  <meta name="description" content="shootout - Shootout test between lua/luajit/java/php" />

  <meta content="56617" name="octolytics-dimension-user_id" /><meta content="chaoslawful" name="octolytics-dimension-user_login" /><meta content="4901618" name="octolytics-dimension-repository_id" /><meta content="chaoslawful/shootout" name="octolytics-dimension-repository_nwo" /><meta content="true" name="octolytics-dimension-repository_public" /><meta content="false" name="octolytics-dimension-repository_is_fork" /><meta content="4901618" name="octolytics-dimension-repository_network_root_id" /><meta content="chaoslawful/shootout" name="octolytics-dimension-repository_network_root_nwo" />
  <link href="https://github.com/chaoslawful/shootout/commits/master.atom" rel="alternate" title="Recent Commits to shootout:master" type="application/atom+xml" />

  </head>


  <body class="logged_out  env-production windows vis-public page-blob">
    <div class="wrapper">
      
      
      


      
      <div class="header header-logged-out">
  <div class="container clearfix">

    <a class="header-logo-wordmark" href="https://github.com/">
      <span class="mega-octicon octicon-logo-github"></span>
    </a>

    <div class="header-actions">
        <a class="button primary" href="/signup">Sign up</a>
      <a class="button" href="/login?return_to=%2Fchaoslawful%2Fshootout%2Fblob%2Fmaster%2Fspectralnorm.lua">Sign in</a>
    </div>

    <div class="command-bar js-command-bar  in-repository">

      <ul class="top-nav">
          <li class="explore"><a href="/explore">Explore</a></li>
        <li class="features"><a href="/features">Features</a></li>
          <li class="enterprise"><a href="https://enterprise.github.com/">Enterprise</a></li>
          <li class="blog"><a href="/blog">Blog</a></li>
      </ul>
        <form accept-charset="UTF-8" action="/search" class="command-bar-form" id="top_search_form" method="get">

<input type="text" data-hotkey="/ s" name="q" id="js-command-bar-field" placeholder="Search or type a command" tabindex="1" autocapitalize="off"
    
    
      data-repo="chaoslawful/shootout"
      data-branch="master"
      data-sha="5aebc1e0de1826a43d180caa9394eb1e58136879"
  >

    <input type="hidden" name="nwo" value="chaoslawful/shootout" />

    <div class="select-menu js-menu-container js-select-menu search-context-select-menu">
      <span class="minibutton select-menu-button js-menu-target">
        <span class="js-select-button">This repository</span>
      </span>

      <div class="select-menu-modal-holder js-menu-content js-navigation-container">
        <div class="select-menu-modal">

          <div class="select-menu-item js-navigation-item js-this-repository-navigation-item selected">
            <span class="select-menu-item-icon octicon octicon-check"></span>
            <input type="radio" class="js-search-this-repository" name="search_target" value="repository" checked="checked" />
            <div class="select-menu-item-text js-select-button-text">This repository</div>
          </div> <!-- /.select-menu-item -->

          <div class="select-menu-item js-navigation-item js-all-repositories-navigation-item">
            <span class="select-menu-item-icon octicon octicon-check"></span>
            <input type="radio" name="search_target" value="global" />
            <div class="select-menu-item-text js-select-button-text">All repositories</div>
          </div> <!-- /.select-menu-item -->

        </div>
      </div>
    </div>

  <span class="octicon help tooltipped downwards" title="Show command bar help">
    <span class="octicon octicon-question"></span>
  </span>


  <input type="hidden" name="ref" value="cmdform">

</form>
    </div>

  </div>
</div>


      


          <div class="site" itemscope itemtype="http://schema.org/WebPage">
    
    <div class="pagehead repohead instapaper_ignore readability-menu">
      <div class="container">
        

<ul class="pagehead-actions">


  <li>
  <a href="/login?return_to=%2Fchaoslawful%2Fshootout"
  class="minibutton with-count js-toggler-target star-button entice tooltipped upwards"
  title="You must be signed in to use this feature" rel="nofollow">
  <span class="octicon octicon-star"></span>Star
</a>
<a class="social-count js-social-count" href="/chaoslawful/shootout/stargazers">
  2
</a>

  </li>

    <li>
      <a href="/login?return_to=%2Fchaoslawful%2Fshootout"
        class="minibutton with-count js-toggler-target fork-button entice tooltipped upwards"
        title="You must be signed in to fork a repository" rel="nofollow">
        <span class="octicon octicon-git-branch"></span>Fork
      </a>
      <a href="/chaoslawful/shootout/network" class="social-count">
        3
      </a>
    </li>
</ul>

        <h1 itemscope itemtype="http://data-vocabulary.org/Breadcrumb" class="entry-title public">
          <span class="repo-label"><span>public</span></span>
          <span class="mega-octicon octicon-repo"></span>
          <span class="author">
            <a href="/chaoslawful" class="url fn" itemprop="url" rel="author"><span itemprop="title">chaoslawful</span></a></span
          ><span class="repohead-name-divider">/</span><strong
          ><a href="/chaoslawful/shootout" class="js-current-repository js-repo-home-link">shootout</a></strong>

          <span class="page-context-loader">
            <img alt="Octocat-spinner-32" height="16" src="https://github.global.ssl.fastly.net/images/spinners/octocat-spinner-32.gif" width="16" />
          </span>

        </h1>
      </div><!-- /.container -->
    </div><!-- /.repohead -->

    <div class="container">

      <div class="repository-with-sidebar repo-container ">

        <div class="repository-sidebar">
            

<div class="repo-nav repo-nav-full js-repository-container-pjax js-octicon-loaders">
  <div class="repo-nav-contents">
    <ul class="repo-menu">
      <li class="tooltipped leftwards" title="Code">
        <a href="/chaoslawful/shootout" aria-label="Code" class="js-selected-navigation-item selected" data-gotokey="c" data-pjax="true" data-selected-links="repo_source repo_downloads repo_commits repo_tags repo_branches /chaoslawful/shootout">
          <span class="octicon octicon-code"></span> <span class="full-word">Code</span>
          <img alt="Octocat-spinner-32" class="mini-loader" height="16" src="https://github.global.ssl.fastly.net/images/spinners/octocat-spinner-32.gif" width="16" />
</a>      </li>

        <li class="tooltipped leftwards" title="Issues">
          <a href="/chaoslawful/shootout/issues" aria-label="Issues" class="js-selected-navigation-item js-disable-pjax" data-gotokey="i" data-selected-links="repo_issues /chaoslawful/shootout/issues">
            <span class="octicon octicon-issue-opened"></span> <span class="full-word">Issues</span>
            <span class='counter'>0</span>
            <img alt="Octocat-spinner-32" class="mini-loader" height="16" src="https://github.global.ssl.fastly.net/images/spinners/octocat-spinner-32.gif" width="16" />
</a>        </li>

      <li class="tooltipped leftwards" title="Pull Requests"><a href="/chaoslawful/shootout/pulls" aria-label="Pull Requests" class="js-selected-navigation-item js-disable-pjax" data-gotokey="p" data-selected-links="repo_pulls /chaoslawful/shootout/pulls">
            <span class="octicon octicon-git-pull-request"></span> <span class="full-word">Pull Requests</span>
            <span class='counter'>0</span>
            <img alt="Octocat-spinner-32" class="mini-loader" height="16" src="https://github.global.ssl.fastly.net/images/spinners/octocat-spinner-32.gif" width="16" />
</a>      </li>


    </ul>
    <div class="repo-menu-separator"></div>
    <ul class="repo-menu">

      <li class="tooltipped leftwards" title="Pulse">
        <a href="/chaoslawful/shootout/pulse" aria-label="Pulse" class="js-selected-navigation-item " data-pjax="true" data-selected-links="pulse /chaoslawful/shootout/pulse">
          <span class="octicon octicon-pulse"></span> <span class="full-word">Pulse</span>
          <img alt="Octocat-spinner-32" class="mini-loader" height="16" src="https://github.global.ssl.fastly.net/images/spinners/octocat-spinner-32.gif" width="16" />
</a>      </li>

      <li class="tooltipped leftwards" title="Graphs">
        <a href="/chaoslawful/shootout/graphs" aria-label="Graphs" class="js-selected-navigation-item " data-pjax="true" data-selected-links="repo_graphs repo_contributors /chaoslawful/shootout/graphs">
          <span class="octicon octicon-graph"></span> <span class="full-word">Graphs</span>
          <img alt="Octocat-spinner-32" class="mini-loader" height="16" src="https://github.global.ssl.fastly.net/images/spinners/octocat-spinner-32.gif" width="16" />
</a>      </li>

      <li class="tooltipped leftwards" title="Network">
        <a href="/chaoslawful/shootout/network" aria-label="Network" class="js-selected-navigation-item js-disable-pjax" data-selected-links="repo_network /chaoslawful/shootout/network">
          <span class="octicon octicon-git-branch"></span> <span class="full-word">Network</span>
          <img alt="Octocat-spinner-32" class="mini-loader" height="16" src="https://github.global.ssl.fastly.net/images/spinners/octocat-spinner-32.gif" width="16" />
</a>      </li>
    </ul>


  </div>
</div>

            <div class="only-with-full-nav">
              

  

<div class="clone-url open"
  data-protocol-type="http"
  data-url="/users/set_protocol?protocol_selector=http&amp;protocol_type=clone">
  <h3><strong>HTTPS</strong> clone URL</h3>
  <div class="clone-url-box">
    <input type="text" class="clone js-url-field"
           value="https://github.com/chaoslawful/shootout.git" readonly="readonly">

    <span class="js-zeroclipboard url-box-clippy minibutton zeroclipboard-button" data-clipboard-text="https://github.com/chaoslawful/shootout.git" data-copied-hint="copied!" title="copy to clipboard"><span class="octicon octicon-clippy"></span></span>
  </div>
</div>

  

<div class="clone-url "
  data-protocol-type="subversion"
  data-url="/users/set_protocol?protocol_selector=subversion&amp;protocol_type=clone">
  <h3><strong>Subversion</strong> checkout URL</h3>
  <div class="clone-url-box">
    <input type="text" class="clone js-url-field"
           value="https://github.com/chaoslawful/shootout" readonly="readonly">

    <span class="js-zeroclipboard url-box-clippy minibutton zeroclipboard-button" data-clipboard-text="https://github.com/chaoslawful/shootout" data-copied-hint="copied!" title="copy to clipboard"><span class="octicon octicon-clippy"></span></span>
  </div>
</div>


<p class="clone-options">You can clone with
      <a href="#" class="js-clone-selector" data-protocol="http">HTTPS</a>,
      or <a href="#" class="js-clone-selector" data-protocol="subversion">Subversion</a>.
  <span class="octicon help tooltipped upwards" title="Get help on which URL is right for you.">
    <a href="https://help.github.com/articles/which-remote-url-should-i-use">
    <span class="octicon octicon-question"></span>
    </a>
  </span>
</p>


  <a href="http://windows.github.com" class="minibutton sidebar-button">
    <span class="octicon octicon-device-desktop"></span>
    Clone in Desktop
  </a>

                <a href="/chaoslawful/shootout/archive/master.zip"
                   class="minibutton sidebar-button"
                   title="Download this repository as a zip file"
                   rel="nofollow">
                  <span class="octicon octicon-cloud-download"></span>
                  Download ZIP
                </a>
            </div>
        </div><!-- /.repository-sidebar -->

        <div id="js-repo-pjax-container" class="repository-content context-loader-container" data-pjax-container>
          


<!-- blob contrib key: blob_contributors:v21:e167baa33723ee198369a8d123226252 -->
<!-- blob contrib frag key: views10/v8/blob_contributors:v21:e167baa33723ee198369a8d123226252 -->

<p title="This is a placeholder element" class="js-history-link-replace hidden"></p>

<a href="/chaoslawful/shootout/find/master" data-pjax data-hotkey="t" style="display:none">Show File Finder</a>

<div class="file-navigation">
  


<div class="select-menu js-menu-container js-select-menu" >
  <span class="minibutton select-menu-button js-menu-target" data-hotkey="w"
    data-master-branch="master"
    data-ref="master"
    role="button" aria-label="Switch branches or tags" tabindex="0">
    <span class="octicon octicon-git-branch"></span>
    <i>branch:</i>
    <span class="js-select-button">master</span>
  </span>

  <div class="select-menu-modal-holder js-menu-content js-navigation-container" data-pjax>

    <div class="select-menu-modal">
      <div class="select-menu-header">
        <span class="select-menu-title">Switch branches/tags</span>
        <span class="octicon octicon-remove-close js-menu-close"></span>
      </div> <!-- /.select-menu-header -->

      <div class="select-menu-filters">
        <div class="select-menu-text-filter">
          <input type="text" aria-label="Filter branches/tags" id="context-commitish-filter-field" class="js-filterable-field js-navigation-enable" placeholder="Filter branches/tags">
        </div>
        <div class="select-menu-tabs">
          <ul>
            <li class="select-menu-tab">
              <a href="#" data-tab-filter="branches" class="js-select-menu-tab">Branches</a>
            </li>
            <li class="select-menu-tab">
              <a href="#" data-tab-filter="tags" class="js-select-menu-tab">Tags</a>
            </li>
          </ul>
        </div><!-- /.select-menu-tabs -->
      </div><!-- /.select-menu-filters -->

      <div class="select-menu-list select-menu-tab-bucket js-select-menu-tab-bucket" data-tab-filter="branches">

        <div data-filterable-for="context-commitish-filter-field" data-filterable-type="substring">


            <div class="select-menu-item js-navigation-item selected">
              <span class="select-menu-item-icon octicon octicon-check"></span>
              <a href="/chaoslawful/shootout/blob/master/spectralnorm.lua" class="js-navigation-open select-menu-item-text js-select-button-text css-truncate-target" data-name="master" data-skip-pjax="true" rel="nofollow" title="master">master</a>
            </div> <!-- /.select-menu-item -->
        </div>

          <div class="select-menu-no-results">Nothing to show</div>
      </div> <!-- /.select-menu-list -->

      <div class="select-menu-list select-menu-tab-bucket js-select-menu-tab-bucket" data-tab-filter="tags">
        <div data-filterable-for="context-commitish-filter-field" data-filterable-type="substring">


        </div>

        <div class="select-menu-no-results">Nothing to show</div>
      </div> <!-- /.select-menu-list -->

    </div> <!-- /.select-menu-modal -->
  </div> <!-- /.select-menu-modal-holder -->
</div> <!-- /.select-menu -->

  <div class="breadcrumb">
    <span class='repo-root js-repo-root'><span itemscope="" itemtype="http://data-vocabulary.org/Breadcrumb"><a href="/chaoslawful/shootout" data-branch="master" data-direction="back" data-pjax="true" itemscope="url"><span itemprop="title">shootout</span></a></span></span><span class="separator"> / </span><strong class="final-path">spectralnorm.lua</strong> <span class="js-zeroclipboard minibutton zeroclipboard-button" data-clipboard-text="spectralnorm.lua" data-copied-hint="copied!" title="copy to clipboard"><span class="octicon octicon-clippy"></span></span>
  </div>
</div>


  
  <div class="commit file-history-tease">
    <img class="main-avatar" height="24" src="https://1.gravatar.com/avatar/a823b0601776070b345e2f8ce44b5337?d=https%3A%2F%2Fidenticons.github.com%2F956c5963bbe915d03b855c0bd0d1a822.png&amp;s=140" width="24" />
    <span class="author"><a href="/chaoslawful" rel="author">chaoslawful</a></span>
    <time class="js-relative-date" datetime="2012-07-05T01:35:21-07:00" title="2012-07-05 01:35:21">July 05, 2012</time>
    <div class="commit-title">
        <a href="/chaoslawful/shootout/commit/6b5df60d5cc5665196c25972f585d3a31bb77637" class="message" data-pjax="true" title="initial commit">initial commit</a>
    </div>

    <div class="participation">
      <p class="quickstat"><a href="#blob_contributors_box" rel="facebox"><strong>1</strong> contributor</a></p>
      
    </div>
    <div id="blob_contributors_box" style="display:none">
      <h2 class="facebox-header">Users who have contributed to this file</h2>
      <ul class="facebox-user-list">
        <li class="facebox-user-list-item">
          <img height="24" src="https://1.gravatar.com/avatar/a823b0601776070b345e2f8ce44b5337?d=https%3A%2F%2Fidenticons.github.com%2F956c5963bbe915d03b855c0bd0d1a822.png&amp;s=140" width="24" />
          <a href="/chaoslawful">chaoslawful</a>
        </li>
      </ul>
    </div>
  </div>


<div id="files" class="bubble">
  <div class="file">
    <div class="meta">
      <div class="info">
        <span class="icon"><b class="octicon octicon-file-text"></b></span>
        <span class="mode" title="File Mode">file</span>
          <span>45 lines (36 sloc)</span>
        <span>0.8 kb</span>
      </div>
      <div class="actions">
        <div class="button-group">
              <a class="minibutton disabled js-entice" href=""
                 data-entice="You must be signed in to make or propose changes">Edit</a>
          <a href="/chaoslawful/shootout/raw/master/spectralnorm.lua" class="button minibutton " id="raw-url">Raw</a>
            <a href="/chaoslawful/shootout/blame/master/spectralnorm.lua" class="button minibutton ">Blame</a>
          <a href="/chaoslawful/shootout/commits/master/spectralnorm.lua" class="button minibutton " rel="nofollow">History</a>
        </div><!-- /.button-group -->
            <a class="minibutton danger empty-icon js-entice" href=""
               data-entice="You must be signed in and on a branch to make or propose changes">
            Delete
          </a>
      </div><!-- /.actions -->

    </div>
        <div class="blob-wrapper data type-lua js-blob-data">
        <table class="file-code file-diff">
          <tr class="file-code-line">
            <td class="blob-line-nums">
              <span id="L1" rel="#L1">1</span>
<span id="L2" rel="#L2">2</span>
<span id="L3" rel="#L3">3</span>
<span id="L4" rel="#L4">4</span>
<span id="L5" rel="#L5">5</span>
<span id="L6" rel="#L6">6</span>
<span id="L7" rel="#L7">7</span>
<span id="L8" rel="#L8">8</span>
<span id="L9" rel="#L9">9</span>
<span id="L10" rel="#L10">10</span>
<span id="L11" rel="#L11">11</span>
<span id="L12" rel="#L12">12</span>
<span id="L13" rel="#L13">13</span>
<span id="L14" rel="#L14">14</span>
<span id="L15" rel="#L15">15</span>
<span id="L16" rel="#L16">16</span>
<span id="L17" rel="#L17">17</span>
<span id="L18" rel="#L18">18</span>
<span id="L19" rel="#L19">19</span>
<span id="L20" rel="#L20">20</span>
<span id="L21" rel="#L21">21</span>
<span id="L22" rel="#L22">22</span>
<span id="L23" rel="#L23">23</span>
<span id="L24" rel="#L24">24</span>
<span id="L25" rel="#L25">25</span>
<span id="L26" rel="#L26">26</span>
<span id="L27" rel="#L27">27</span>
<span id="L28" rel="#L28">28</span>
<span id="L29" rel="#L29">29</span>
<span id="L30" rel="#L30">30</span>
<span id="L31" rel="#L31">31</span>
<span id="L32" rel="#L32">32</span>
<span id="L33" rel="#L33">33</span>
<span id="L34" rel="#L34">34</span>
<span id="L35" rel="#L35">35</span>
<span id="L36" rel="#L36">36</span>
<span id="L37" rel="#L37">37</span>
<span id="L38" rel="#L38">38</span>
<span id="L39" rel="#L39">39</span>
<span id="L40" rel="#L40">40</span>
<span id="L41" rel="#L41">41</span>
<span id="L42" rel="#L42">42</span>
<span id="L43" rel="#L43">43</span>
<span id="L44" rel="#L44">44</span>

            </td>
            <td class="blob-line-code">
                    <div class="highlight"><pre><div class='line' id='LC1'><span class="c1">-- The Computer Language Benchmarks Game</span></div><div class='line' id='LC2'><span class="c1">-- http://shootout.alioth.debian.org/</span></div><div class='line' id='LC3'><span class="c1">-- contributed by Mike Pall</span></div><div class='line' id='LC4'><br/></div><div class='line' id='LC5'><span class="kd">local</span> <span class="k">function</span> <span class="nf">A</span><span class="p">(</span><span class="n">i</span><span class="p">,</span> <span class="n">j</span><span class="p">)</span></div><div class='line' id='LC6'>	<span class="kd">local</span> <span class="n">ij</span> <span class="o">=</span> <span class="n">i</span><span class="o">+</span><span class="n">j</span><span class="o">-</span><span class="mi">1</span></div><div class='line' id='LC7'>	<span class="k">return</span> <span class="mf">1.0</span> <span class="o">/</span> <span class="p">(</span><span class="n">ij</span> <span class="o">*</span> <span class="p">(</span><span class="n">ij</span><span class="o">-</span><span class="mi">1</span><span class="p">)</span> <span class="o">*</span> <span class="mf">0.5</span> <span class="o">+</span> <span class="n">i</span><span class="p">)</span></div><div class='line' id='LC8'><span class="k">end</span></div><div class='line' id='LC9'><br/></div><div class='line' id='LC10'><span class="kd">local</span> <span class="k">function</span> <span class="nf">Av</span><span class="p">(</span><span class="n">x</span><span class="p">,</span> <span class="n">y</span><span class="p">,</span> <span class="n">N</span><span class="p">)</span></div><div class='line' id='LC11'>	<span class="k">for</span> <span class="n">i</span><span class="o">=</span><span class="mi">1</span><span class="p">,</span><span class="n">N</span> <span class="k">do</span></div><div class='line' id='LC12'>		<span class="kd">local</span> <span class="n">a</span> <span class="o">=</span> <span class="mi">0</span></div><div class='line' id='LC13'>		<span class="k">for</span> <span class="n">j</span><span class="o">=</span><span class="mi">1</span><span class="p">,</span><span class="n">N</span> <span class="k">do</span> <span class="n">a</span> <span class="o">=</span> <span class="n">a</span> <span class="o">+</span> <span class="n">x</span><span class="p">[</span><span class="n">j</span><span class="p">]</span> <span class="o">*</span> <span class="n">A</span><span class="p">(</span><span class="n">i</span><span class="p">,</span> <span class="n">j</span><span class="p">)</span> <span class="k">end</span></div><div class='line' id='LC14'>		<span class="n">y</span><span class="p">[</span><span class="n">i</span><span class="p">]</span> <span class="o">=</span> <span class="n">a</span></div><div class='line' id='LC15'>	<span class="k">end</span></div><div class='line' id='LC16'><span class="k">end</span></div><div class='line' id='LC17'><br/></div><div class='line' id='LC18'><span class="kd">local</span> <span class="k">function</span> <span class="nf">Atv</span><span class="p">(</span><span class="n">x</span><span class="p">,</span> <span class="n">y</span><span class="p">,</span> <span class="n">N</span><span class="p">)</span></div><div class='line' id='LC19'>	<span class="k">for</span> <span class="n">i</span><span class="o">=</span><span class="mi">1</span><span class="p">,</span><span class="n">N</span> <span class="k">do</span></div><div class='line' id='LC20'>		<span class="kd">local</span> <span class="n">a</span> <span class="o">=</span> <span class="mi">0</span></div><div class='line' id='LC21'>		<span class="k">for</span> <span class="n">j</span><span class="o">=</span><span class="mi">1</span><span class="p">,</span><span class="n">N</span> <span class="k">do</span> <span class="n">a</span> <span class="o">=</span> <span class="n">a</span> <span class="o">+</span> <span class="n">x</span><span class="p">[</span><span class="n">j</span><span class="p">]</span> <span class="o">*</span> <span class="n">A</span><span class="p">(</span><span class="n">j</span><span class="p">,</span> <span class="n">i</span><span class="p">)</span> <span class="k">end</span></div><div class='line' id='LC22'>		<span class="n">y</span><span class="p">[</span><span class="n">i</span><span class="p">]</span> <span class="o">=</span> <span class="n">a</span></div><div class='line' id='LC23'>	<span class="k">end</span></div><div class='line' id='LC24'><span class="k">end</span></div><div class='line' id='LC25'><br/></div><div class='line' id='LC26'><span class="kd">local</span> <span class="k">function</span> <span class="nf">AtAv</span><span class="p">(</span><span class="n">x</span><span class="p">,</span> <span class="n">y</span><span class="p">,</span> <span class="n">t</span><span class="p">,</span> <span class="n">N</span><span class="p">)</span></div><div class='line' id='LC27'>	<span class="n">Av</span><span class="p">(</span><span class="n">x</span><span class="p">,</span> <span class="n">t</span><span class="p">,</span> <span class="n">N</span><span class="p">)</span></div><div class='line' id='LC28'>	<span class="n">Atv</span><span class="p">(</span><span class="n">t</span><span class="p">,</span> <span class="n">y</span><span class="p">,</span> <span class="n">N</span><span class="p">)</span></div><div class='line' id='LC29'><span class="k">end</span></div><div class='line' id='LC30'><br/></div><div class='line' id='LC31'><span class="kd">local</span> <span class="n">N</span> <span class="o">=</span> <span class="nb">tonumber</span><span class="p">(</span><span class="n">arg</span> <span class="ow">and</span> <span class="n">arg</span><span class="p">[</span><span class="mi">1</span><span class="p">])</span> <span class="ow">or</span> <span class="mi">100</span></div><div class='line' id='LC32'><span class="kd">local</span> <span class="n">u</span><span class="p">,</span> <span class="n">v</span><span class="p">,</span> <span class="n">t</span> <span class="o">=</span> <span class="p">{},</span> <span class="p">{},</span> <span class="p">{}</span></div><div class='line' id='LC33'><span class="k">for</span> <span class="n">i</span><span class="o">=</span><span class="mi">1</span><span class="p">,</span><span class="n">N</span> <span class="k">do</span> <span class="n">u</span><span class="p">[</span><span class="n">i</span><span class="p">]</span> <span class="o">=</span> <span class="mi">1</span> <span class="k">end</span></div><div class='line' id='LC34'><br/></div><div class='line' id='LC35'><span class="k">for</span> <span class="n">i</span><span class="o">=</span><span class="mi">1</span><span class="p">,</span><span class="mi">10</span> <span class="k">do</span> <span class="n">AtAv</span><span class="p">(</span><span class="n">u</span><span class="p">,</span> <span class="n">v</span><span class="p">,</span> <span class="n">t</span><span class="p">,</span> <span class="n">N</span><span class="p">)</span> <span class="n">AtAv</span><span class="p">(</span><span class="n">v</span><span class="p">,</span> <span class="n">u</span><span class="p">,</span> <span class="n">t</span><span class="p">,</span> <span class="n">N</span><span class="p">)</span> <span class="k">end</span></div><div class='line' id='LC36'><br/></div><div class='line' id='LC37'><span class="kd">local</span> <span class="n">vBv</span><span class="p">,</span> <span class="n">vv</span> <span class="o">=</span> <span class="mi">0</span><span class="p">,</span> <span class="mi">0</span></div><div class='line' id='LC38'><span class="k">for</span> <span class="n">i</span><span class="o">=</span><span class="mi">1</span><span class="p">,</span><span class="n">N</span> <span class="k">do</span></div><div class='line' id='LC39'>	<span class="kd">local</span> <span class="n">ui</span><span class="p">,</span> <span class="n">vi</span> <span class="o">=</span> <span class="n">u</span><span class="p">[</span><span class="n">i</span><span class="p">],</span> <span class="n">v</span><span class="p">[</span><span class="n">i</span><span class="p">]</span></div><div class='line' id='LC40'>	<span class="n">vBv</span> <span class="o">=</span> <span class="n">vBv</span> <span class="o">+</span> <span class="n">ui</span><span class="o">*</span><span class="n">vi</span></div><div class='line' id='LC41'>	<span class="n">vv</span> <span class="o">=</span> <span class="n">vv</span> <span class="o">+</span> <span class="n">vi</span><span class="o">*</span><span class="n">vi</span></div><div class='line' id='LC42'><span class="k">end</span></div><div class='line' id='LC43'><span class="nb">io.write</span><span class="p">(</span><span class="nb">string.format</span><span class="p">(</span><span class="s2">&quot;</span><span class="s">%0.9f</span><span class="se">\n</span><span class="s">&quot;</span><span class="p">,</span> <span class="nb">math.sqrt</span><span class="p">(</span><span class="n">vBv</span> <span class="o">/</span> <span class="n">vv</span><span class="p">)))</span></div><div class='line' id='LC44'><br/></div></pre></div>
            </td>
          </tr>
        </table>
  </div>

  </div>
</div>

<a href="#jump-to-line" rel="facebox[.linejump]" data-hotkey="l" class="js-jump-to-line" style="display:none">Jump to Line</a>
<div id="jump-to-line" style="display:none">
  <form accept-charset="UTF-8" class="js-jump-to-line-form">
    <input class="linejump-input js-jump-to-line-field" type="text" placeholder="Jump to line&hellip;" autofocus>
    <button type="submit" class="button">Go</button>
  </form>
</div>

        </div>

      </div><!-- /.repo-container -->
      <div class="modal-backdrop"></div>
    </div><!-- /.container -->
  </div><!-- /.site -->


    </div><!-- /.wrapper -->

      <div class="container">
  <div class="site-footer">
    <ul class="site-footer-links right">
      <li><a href="https://status.github.com/">Status</a></li>
      <li><a href="http://developer.github.com">API</a></li>
      <li><a href="http://training.github.com">Training</a></li>
      <li><a href="http://shop.github.com">Shop</a></li>
      <li><a href="/blog">Blog</a></li>
      <li><a href="/about">About</a></li>

    </ul>

    <a href="/">
      <span class="mega-octicon octicon-mark-github"></span>
    </a>

    <ul class="site-footer-links">
      <li>&copy; 2013 <span title="0.08141s from github-fe116-cp1-prd.iad.github.net">GitHub</span>, Inc.</li>
        <li><a href="/site/terms">Terms</a></li>
        <li><a href="/site/privacy">Privacy</a></li>
        <li><a href="/security">Security</a></li>
        <li><a href="/contact">Contact</a></li>
    </ul>
  </div><!-- /.site-footer -->
</div><!-- /.container -->


    <div class="fullscreen-overlay js-fullscreen-overlay" id="fullscreen_overlay">
  <div class="fullscreen-container js-fullscreen-container">
    <div class="textarea-wrap">
      <textarea name="fullscreen-contents" id="fullscreen-contents" class="js-fullscreen-contents" placeholder="" data-suggester="fullscreen_suggester"></textarea>
          <div class="suggester-container">
              <div class="suggester fullscreen-suggester js-navigation-container" id="fullscreen_suggester"
                 data-url="/chaoslawful/shootout/suggestions/commit">
              </div>
          </div>
    </div>
  </div>
  <div class="fullscreen-sidebar">
    <a href="#" class="exit-fullscreen js-exit-fullscreen tooltipped leftwards" title="Exit Zen Mode">
      <span class="mega-octicon octicon-screen-normal"></span>
    </a>
    <a href="#" class="theme-switcher js-theme-switcher tooltipped leftwards"
      title="Switch themes">
      <span class="octicon octicon-color-mode"></span>
    </a>
  </div>
</div>



    <div id="ajax-error-message" class="flash flash-error">
      <span class="octicon octicon-alert"></span>
      <a href="#" class="octicon octicon-remove-close close ajax-error-dismiss"></a>
      Something went wrong with that request. Please try again.
    </div>

    
  </body>
</html>

