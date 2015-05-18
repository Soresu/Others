// ==UserScript==
// @name        Joduska.me
// @namespace   https://www.joduska.me
// @include     https://www.joduska.me/forum/
// @include     https://www.joduska.me/forum/shoutbox/
// @version     1
// @grant       none
// ==/UserScript==

//You can change the settings here
var refreshingTimeInSeconds=60; // Default time, you can change with the GUI
var CheckNotification=true; // Notifications with the globe icon
var CheckMessage=true; // Notifications with the mail icon
var directUrlForAlerSound='https://raw.githubusercontent.com/Soresu/Others/master/Sounds/DefaultAlert.mp3'; // Notification sound
var SoundVolume=1.0; // Notification volume
var NamesToCheckInShoutBox = ["Joduskame", "Exploit", "MyName"];// Sound alert if the ShoutBox contains the keyword, you can add more if you want
//Don't change the next lines


var timeoutID;
var player = document.createElement('audio');
var InitialNames=0;
var MenuAddon='<li><input id="IsSoundEnabled" style="line-height: 30px;outline: medium none;height: 30px;margin: 7px 8px 0px 2px;background: none repeat scroll 0% 0% rgba(0, 0, 0, 0.3);box-shadow: 0px 0px 0px 1px rgba(0, 0, 0, 0.1) inset;padding: 0px 8px;float: right;color: #FFF;" name="Shoutbox" value="1" type="checkbox">ShoutBox sounds: </li><li><input id="RefreshInterval" style="line-height: 30px;outline: medium none;height: 25px;width: 20px;margin: 7px 2px;background: none repeat scroll 0% 0% rgba(0, 0, 0, 0.3);box-shadow: 0px 0px 0px 1px rgba(0, 0, 0, 0.1) inset;padding: 2px 2px;float: right;color: #FFF; text-align:center; border: medium none; border-radius: 3px; " name="RefreshInt" value="60" type="Text">Refresh rate: </li>';

function setup() {
    this.addEventListener("mousemove", resetTimer, false);
    this.addEventListener("mousedown", resetTimer, false);
    this.addEventListener("keypress", resetTimer, false);
    this.addEventListener("DOMMouseScroll", resetTimer, false);
    this.addEventListener("mousewheel", resetTimer, false);
    this.addEventListener("touchmove", resetTimer, false);
    this.addEventListener("MSPointerMove", resetTimer, false);
    AddMenu();
    SetMenu();
    startTimer();
}
setup();
window.onbeforeunload = function(){
  document.cookie="RefreshInterval="+document.getElementById('RefreshInterval').value+"; expires=Thu, 18 Dec 2020 12:00:00 UTC";
     document.cookie="IsSoundEnabled="+document.getElementById("IsSoundEnabled").checked+"; expires=Thu, 18 Dec 2020 12:00:00 UTC";
};
function SetMenu() {
  if(getCookie("RefreshInterval").length>0){
    document.getElementById('RefreshInterval').value =getCookie("RefreshInterval");
  }
    var soundBool=getCookie("IsSoundEnabled");
  if(soundBool.length>0){
      if(soundBool.toLowerCase()=="true"){
          document.getElementById("IsSoundEnabled").checked=true; 
      }else{
         document.getElementById("IsSoundEnabled").checked=false;  
      }
   
  }
   refreshingTimeInSeconds=document.getElementById('RefreshInterval').value;
    if(refreshingTimeInSeconds<5){
        refreshingTimeInSeconds=10;
    }
}
function SoundEnabled() {
   return document.getElementById('IsSoundEnabled').checked;
}
function AddMenu() {
   var NavBar=document.getElementById('user_navigation').getElementsByTagName('ul')[0];
   NavBar.insertAdjacentHTML( 'beforeend', MenuAddon );
}
function Countnames() {
    if(!document.getElementById("category_shoutbox")){
        return 0;
    }
   var ShoutboxText=document.getElementById('shoutbox-shouts-table').innerHTML;
   var len = NamesToCheckInShoutBox.length;
   var count=0;
   for (i=0; i < len; i++) {
     var re = new RegExp(NamesToCheckInShoutBox[i],"g");
     count = count + (ShoutboxText.match(re) || []).length;
   }
   return count;
}
function CheckNotifications() {
    var shouldNotify=false;
    var reg = new RegExp("ipsHasNotifications","g");
    if(CheckNotification){
        var mailIcon=document.getElementById('inbox_link').innerHTML;
        if((mailIcon.match(reg) || []).length>0){
          shouldNotify=true;
        }
        
    }
    if(CheckMessage){
        var globeIcon=document.getElementById('notify_link').innerHTML;
        if((globeIcon.match(reg) || []).length>0){
          shouldNotify=true;
        }
    }
    return shouldNotify;
}
function countInstances(string, word) {
   var substrings = string.split(word);
   return substrings.length - 1;
}
function startTimer() {
    InitialNames=Countnames();
    timeoutID = window.setTimeout(goInactive, refreshingTimeInSeconds*1000);
}
 
function resetTimer(e) {
    window.clearTimeout(timeoutID);
    goActive();
}
function AddMenu() {
   var NavBar=document.getElementById('user_navigation').getElementsByTagName('ul')[0];
   NavBar.insertAdjacentHTML( 'beforeend', MenuAddon );
}
function goInactive() {
  var SoundAlert=false;
  var ShoutboxText=Countnames();
  if((Countnames()>InitialNames && SoundEnabled()) || CheckNotifications()){
    SoundAlert=true;
  }
  if(SoundAlert){
    player.src = directUrlForAlerSound;
    player.preload = 'auto';
    player.volume=SoundVolume;
    player.play();
  }
  setTimeout(function(){ location.reload(); }, 1*1000);
}
function goActive() {
    startTimer();
}
function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for(var i=0; i<ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0)==' ') c = c.substring(1);
        if (c.indexOf(name) == 0) return c.substring(name.length,c.length);
    }
    return "";
}