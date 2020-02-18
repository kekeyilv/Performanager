document.getElementById("content").style.marginTop = (this.document.getElementsByTagName("nav")[0].clientHeight + 5).toString() + "px";
setInterval(function () {
    var content = document.getElementById("content");
    content.style.marginTop = (this.document.getElementsByTagName("nav")[0].clientHeight + 5).toString() + "px";
}, 10);

function AdaptManagePage() {
    setInterval(function () {
        var bar = document.getElementById("bar");
        var content = document.getElementById("innercontent");
        content.style.top=bar.style.top = (this.document.getElementsByTagName("nav")[0].clientHeight + 5).toString() + "px"
    }, 10);
}