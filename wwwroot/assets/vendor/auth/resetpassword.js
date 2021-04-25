function processForm(e) {
    if (e.preventDefault) e.preventDefault();

    /* do what you want with the form */
    if(document.getElementById("password").value == document.getElementById("confirmpassword").value && document.getElementById("password").value  != '') {
        httpPostAsync()
    } else {
        document.getElementById("error").innerHTML="<div class='error' >Passwords don't match </div>"
    }
    // You must return false to prevent the default form behavior
    return false;
}
function showPassword(){
    "password" === document.getElementById("password").type ? (document.getElementById("password").type = "text", t.classList.remove("icon-eye"), t.classList.add("icon-eye-solid"), e.innerText = "Hide password") : (document.getElementById("password").type = "password", t.classList.remove("icon-eye-solid"), t.classList.add("icon-eye"), e.innerText = "Show password")
}
function showCPassword(){
    "password" === document.getElementById("confirmpassword").type ? (document.getElementById("confirmpassword").type = "text", t.classList.remove("icon-eye"), t.classList.add("icon-eye-solid"), e.innerText = "Hide password") : (document.getElementById("confirmpassword").type = "password", t.classList.remove("icon-eye-solid"), t.classList.add("icon-eye"), e.innerText = "Show password")
}

var form = document.getElementById('reset');
if (form.attachEvent) {
    form.attachEvent("submit", processForm);
} else {
    form.addEventListener("submit", processForm);
}
async function httpPostAsync(){
  var http = new XMLHttpRequest();
    var url = 'http://ericssonquiz.com/IMK/Tests/changedashboardpassword.php';
    
    var params = 'Email='+document.getElementById("username").value+'&Password='+document.getElementById("password").value;
    http.open('POST', url, true);
    
    //Send the proper header information along with the request
    http.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');
    
    http.onreadystatechange = function() {//Call a function when the state changes.
        if(http.readyState == 4 && http.status == 200) {
            const json = http.responseText;
            const obj = JSON.parse(json);
            if(obj.success){
                alert(obj.message)
                window.location.replace("http://ericssonquiz.com/dashboard/login.html");
            }else{
                document.getElementById("error").innerHTML="<div class='error' >"+obj.message+"</div>"
            }
        }
    }
    http.send(params);
};