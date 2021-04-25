document.getElementById("phone").value="+"
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

var form = document.getElementById('auth');
if (form.attachEvent) {
    form.attachEvent("submit", processForm);
} else {
    form.addEventListener("submit", processForm);
}


async function httpPostAsync(){
  var http = new XMLHttpRequest();
    var url = 'http://ericssonquiz.com/IMK/Tests/signup.php';
    var params = 'fromForm=1&Email='+document.getElementById("username").value+'&Password=12345678&CountryName='+document.getElementById("country").value+'&AspName=NA&OperatorName=NA&FullName='+document.getElementById("name").value+'&Phone=+'+document.getElementById("phone").value;
    http.open('POST', url, true);
    
    //Send the proper header information along with the request
    http.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');
    
    http.onreadystatechange = function() {//Call a function when the state changes.
        if(http.readyState == 4 && http.status == 200) {
            const json = http.responseText;
            const obj = JSON.parse(json);
            if(obj.success){
                window.location.replace("http://ericssonquiz.com/dashboard/signup.html");
            }else{
                document.getElementById("error").innerHTML="<div class='error' >"+obj.message+"</div>"
            }
        }
    }
    http.send(params);
};