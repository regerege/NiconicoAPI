[<AutoOpen>]
module Misc

open System
open System.Reflection

[<assembly: AssemblyTitle "RLib.NiconicoAPI";
  assembly: AssemblyDescription "ニコニコ動画簡易API";
  assembly: AssemblyProduct "RLib.NiconicoAPI";
  assembly: AssemblyCopyright "Copyright© 2012 regerege  All Rights Reserved.";
  assembly: AssemblyVersion "0.1.2.*">] ()


///UNIX時間と日本時間の変換をサポート
module UnixTime =
  /// 日本時間とUNIX時間の差
  let UnixEpoch = DateTime(1970, 1, 1, 9, 0, 0, DateTimeKind.Local)
  let ticks = 100000L

  ///UNIX時間をDateTimeに変換する。
  let FromUnixTime time = UnixEpoch.AddSeconds time
  ///UNIX時間をTimeSpanに変換する。
  let FromUnixTimeToTimeSpan time = TimeSpan (time * ticks)
  ///DateTimeをUNIX時間に変換する。
  let ToUnixTime (date:DateTime) = (date - UnixEpoch).TotalSeconds
  ///TimeSpanをUNIX時間に変換する。
  let FromTimeSpanToUnixTime (ts:TimeSpan) = ts.Ticks / ticks


module XmlCommon =
  let toInt n = try Int32.Parse n with _ -> -1
  let toDate (time:int) = string time |> Double.Parse |> UnixTime.FromUnixTime 
  let toTimeSpan str = try Int64.Parse str |> UnixTime.FromUnixTimeToTimeSpan with _ -> TimeSpan.Zero


open FSharp.Data
type ChatResult = XmlProvider<"""<chat_result thread="1288164172" status="0" no="1920"/>""">
type Chat = XmlProvider<"""<chat thread="1288360965" no="1" vpos="55716" date="1375930076" date_usec="778378" user_id="aaa" premium="1">わこつー</chat>""">
type GetPlayerStatus = XmlProvider<"""<?xml version="1.0" encoding="utf-8"?><getplayerstatus status="ok" time="1375932769"><stream><id>lv148064383</id><title>ひよっこ創作人の雑談(2791枠目)。</title><description>主もリスナも一歩前進するために必要な、休息をするための放送。主はアマチュアの創作者です。　創作に関する話題は歓迎です。この放送の詳細については、コミュプロフを参</description><provider_type>community</provider_type><default_community>co1741635</default_community><international>13</international><is_owner>0</is_owner><owner_id>6251677</owner_id><owner_name>みるきあ</owner_name><is_reserved>0</is_reserved><watch_count>10</watch_count><comment_count>57</comment_count><base_time>1375931334</base_time><open_time>1375931334</open_time><start_time>1375931342</start_time><end_time>1375933142</end_time><is_rerun_stream>0</is_rerun_stream><bourbon_url>http://live.nicovideo.jp/gate/lv148064383?sec=nicolive_crowded&amp;sub=watch_crowded_0_community_lv148064383_onair</bourbon_url><full_video>http://live.nicovideo.jp/gate/lv148064383?sec=nicolive_crowded&amp;sub=watch_crowded_0_community_lv148064383_onair</full_video><after_video></after_video><before_video></before_video><kickout_video>http://live.nicovideo.jp/gate/lv148064383?sec=nicolive_oidashi&amp;sub=watchplayer_oidashialert_0_community_lv148064383_onair</kickout_video><twitter_tag>#co1741635</twitter_tag><danjo_comment_mode>0</danjo_comment_mode><infinity_mode>0</infinity_mode><archive>0</archive><press><display_lines>-1</display_lines><display_time>-1</display_time><style_conf></style_conf></press><plugin_delay></plugin_delay><plugin_url></plugin_url><plugin_urls/><allow_netduetto>1</allow_netduetto><nd_token>fcacad3eb354acdaee04f2a476d1f71649c4a276</nd_token><ng_scoring>0</ng_scoring><header_comment>0</header_comment><footer_comment>0</footer_comment><split_bottom>0</split_bottom><split_top>0</split_top><background_comment>0</background_comment><font_scale></font_scale><comment_lock>0</comment_lock><telop><enable>0</enable></telop><contents_list><contents id="main" disableAudio="0" disableVideo="0" start_time="1375931335">rtmp:rtmp://nlpoca29.live.nicovideo.jp:1935/publicorigin/130808_12_1/,lv148064383?1375932769:30:26d4ae9b338cfb71</contents></contents_list><is_priority_prefecture></is_priority_prefecture></stream><user><user_id>someuserid</user_id><nickname>ながと</nickname><is_premium>1</is_premium><userAge>26</userAge><userSex>1</userSex><userDomain>jp</userDomain><userPrefecture>64</userPrefecture><userLanguage>ja-jp</userLanguage><room_label>co1741635</room_label><room_seetno>8</room_seetno><is_join>1</is_join><twitter_info><status>disabled</status><screen_name></screen_name><followers_count>0</followers_count><is_vip>0</is_vip><profile_image_url>http://a1.twimg.com/sticky/default_profile_images/default_profile_6_normal.png</profile_image_url><after_auth>0</after_auth><tweet_token>6c96bb5ba64ea460cef9760d56bce5e02f4c6892</tweet_token></twitter_info></user><rtmp is_fms="1" rtmpt_port="80"><url>rtmp://nleaf11.live.nicovideo.jp:1935/liveedge/live_130808_12_1</url><ticket>67286:lv148064383:0:1375932769:5a46ecfad27496ea</ticket></rtmp><ms><addr>msg103.live.nicovideo.jp</addr><port>2808</port><thread>1288363774</thread></ms><tid_list/><twitter><live_enabled>0</live_enabled><vip_mode_count>10000</vip_mode_count><live_api_url>http://watch.live.nicovideo.jp/api/</live_api_url></twitter><player><error_report>1</error_report></player><marquee><category>一般(その他)</category><game_key>afd55ec9</game_key><game_time>1375932769</game_time><force_nicowari_off>0</force_nicowari_off></marquee></getplayerstatus>""">
type Thread = XmlProvider<"""<thread resultcode="0" thread="1288366260" last_res="260" ticket="0x8439b00" revision="1" server_time="1375933886" />""">


///受信データの判別共用体
type ReciveData =
  | ConnectionStatus of Thread.DomainTypes.Thread
  | CommentResult of ChatResult.DomainTypes.ChatResult
  | Comment of Chat.DomainTypes.Chat
  | Unknown of string

///送受信データの判別共用体
type CommentData = 
  | ReceiveNG of Exception 
  | ReceiveOK of ReciveData 
  | SendNG of Exception 
  | SendOK of string

/// 送信結果ステータス
type ChatResultStatus = 
  | Success = 0 
  | Failure = 1 
  | ThreadIdError = 2 
  | TicketError = 3 
  | PostKeyError = 4 
  | CommentBlockError = 5