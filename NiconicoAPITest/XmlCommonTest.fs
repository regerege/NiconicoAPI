namespace RLib.NiconicoAPI.Xml

open NUnit.Framework
open System

[<TestFixture>]
module NXmlReader =
    let getplayerstatus = @"
<getplayerstatus status=""ok"" time=""1334394106"">
  <stream>
    <id>lv89252500</id>
    <watch_count>15</watch_count>
    <title>【F#】 ニコニコビュアー</title>
    <description>とりあえずコメビュを作ってみます。テスト駆動開発から派生したビヘイビア駆動開発</description>
    <comment_count>2</comment_count>
    <danjo_comment_mode>0</danjo_comment_mode>
    <international>1</international>
    <hqstream>0</hqstream>
    <nicoden>0</nicoden>
    <allow_netduetto>1</allow_netduetto>
    <for_vita>1</for_vita>
    <relay_comment>0</relay_comment>
    <nd_token>94f925422d1940b8739594f5c4611d2360cdd81a</nd_token>
    <bourbon_url>http://live.nicovideo.jp/gate/lv89252500?sec=nicolive_crowded&amp;sub=watch_crowded_0_community_lv89252500_onair</bourbon_url>
    <full_video>http://live.nicovideo.jp/gate/lv89252500?sec=nicolive_crowded&amp;sub=watch_crowded_0_community_lv89252500_onair</full_video>
    <after_video></after_video>
    <before_video></before_video>
    <kickout_video>http://live.nicovideo.jp/gate/lv89252500?sec=nicolive_oidashi&amp;sub=watchplayer_oidashialert_0_community_lv89252500_onair</kickout_video>
    <header_comment>0</header_comment>
    <footer_comment>0</footer_comment>
    <plugin_delay></plugin_delay>
    <plugin_url></plugin_url>
    <font_scale/>
    <infinity_mode>0</infinity_mode>
    <plugin_urls/>
    <provider_type>community</provider_type>
    <default_community>co1397033</default_community>
    <archive>0</archive>
    <is_dj_stream>0</is_dj_stream>
    <is_rerun_stream>0</is_rerun_stream>
    <twitter_tag>#co1397033</twitter_tag>
    <is_owner>1</is_owner>
    <owner_id>1617054</owner_id>
    <owner_name>regerege</owner_name>
    <is_reserved>0</is_reserved>
    <base_time>1334392716</base_time>
    <open_time>1334392716</open_time>
    <end_time>1334394519</end_time>
    <start_time>1334392719</start_time>
    <telop>
      <enable>0</enable>
    </telop>
    <ichiba_notice_enable>0</ichiba_notice_enable>
    <comment_lock>0</comment_lock>
    <background_comment>0</background_comment>
    <split_bottom>0</split_bottom>
    <split_top>0</split_top>
    <contents_list>
      <contents id=""main"" disableAudio=""0"" disableVideo=""0"" start_time=""1334392718"">rtmp:rtmp://nlpoca64.live.nicovideo.jp:1935/publicorigin/120414_17_0/,lv89252500?1334394106:30:c352e18f853a0653</contents>
    </contents_list>
    <press>
      <display_lines>-1</display_lines>
      <display_time>-1</display_time>
      <style_conf/>
    </press>
    <is_priority_prefecture></is_priority_prefecture>
  </stream>
  <user>
    <room_label>co1397033</room_label>
    <room_seetno>2525810</room_seetno>
    <userAge>36</userAge>
    <userSex>1</userSex>
    <userDomain>jp</userDomain>
    <userPrefecture>12</userPrefecture>
    <nickname>regerege</nickname>
    <is_premium>1</is_premium>
    <user_id>1617054</user_id>
    <hkey></hkey>
    <is_join>1</is_join>
    <immu_comment>0</immu_comment>
    <can_broadcast>0</can_broadcast>
    <can_forcelogin>0</can_forcelogin>
    <twitter_info>
      <status>disabled</status>
      <after_auth>0</after_auth>
      <screen_name></screen_name>
      <followers_count>0</followers_count>
      <is_vip>0</is_vip>
      <profile_image_url>http://a5.twimg.com/sticky/default_profile_images/default_profile_3_normal.png</profile_image_url>
      <tweet_token>2a6ee094e5d40361cffd574895e5fa97ed6bb5d5</tweet_token>
    </twitter_info>
  </user>
  <rtmp is_fms=""1"" rtmpt_port=""80"">
    <url>rtmp://nlepa03.live.nicovideo.jp:1935/liveedge/live_120414_18_0</url>
    <ticket>1617054:lv89252500:4:1334394106:f56828f327233853</ticket>
  </rtmp>
  <ms>
    <addr>msg103.live.nicovideo.jp</addr>
    <port>2805</port>
    <thread>1169783505</thread>
  </ms>
  <tid_list>
    <tid>1169783505</tid>
    <tid>1169783506</tid>
  </tid_list>
  <twitter>
    <live_enabled>1</live_enabled>
    <vip_mode_count>10000</vip_mode_count>
    <live_api_url>http://watch.live.nicovideo.jp/api/</live_api_url>
  </twitter>
  <player>
    <error_report>1</error_report>
  </player>
  <marquee>
    <category>一般(その他)</category>
    <game_key>91d416c2</game_key>
    <game_time>1334394106</game_time>
    <force_nicowari_off>0</force_nicowari_off>
  </marquee>
</getplayerstatus>"
    let threadxml = @"
<thread
  resultcode=""0""
  thread=""1169783505""
  last_res=""3""
  ticket=""0xdbf57e0""
  revision=""1""
  server_time=""1334394106""/>"
    let chatxml = @"
<chat
  thread=""1169783505""
  no=""1""
  vpos=""12630""
  date=""1334392842""
  user_id=""20475652""
  premium=""1""
  locale=""jp"">
こんにちわ
</chat>"
    let chatresultxml = @"<chat_result thread=""1169990486"" status=""1""/>"

    [<Test>]
    let ``getplayerstatus XML正常読み込み`` () =
        let gps = XmlReaders.GetPlayerStatusReader getplayerstatus
        Assert.That(gps.Status, Is.EqualTo("ok"))

    [<Test>]
    let ``thread XML正常読み込み`` () =
        let gps = XmlReaders.GetPlayerStatusReader getplayerstatus
        let t = XmlReaders.ThreadReader (gps, threadxml)
//        printfn "%A" t
        Assert.That(t.ServerTime,
            Is.EqualTo(DateTime.Parse("2012-04-14 18:01:46.000")))

    [<Test>]
    let ``chat XML正常読み込み`` () =
        let gps = XmlReaders.GetPlayerStatusReader getplayerstatus
        let t = XmlReaders.ThreadReader (gps, threadxml)
        let c = XmlReaders.ChatReader (gps, t, chatxml)
//        printfn "%A" c
        Assert.That(c.Comment.Trim('\n'), Is.EqualTo("こんにちわ"))

    [<Test>]
    let ``chat_result XML正常読み込み`` () =
        let cr = XmlReaders.ChatResultReader chatresultxml
        Assert.That(cr.Thread, Is.EqualTo("1169990486"))

open RLib.NiconicoAPI.Xml.XmlCommon
open System.Xml
open System.Xml.Linq

[<TestFixture>]
module XmlCommonTest =
    let TestCode xml =
        let root = XElement.Parse xml
        let test = root.Element("test")
        let test = test.Element("test")
        let test = test.Element("test")
        let test = test.Attribute("test")
        test.Value
    [<Test>]
    let ``存在しない要素と属性呼び出しでエラーにならない`` () =
        let value = TestCode NXmlReader.getplayerstatus
        Assert.That(value, Is.Empty)
