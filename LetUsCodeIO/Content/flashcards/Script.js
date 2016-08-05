$(function () {
    //$("#question").hide();
    $("#backcard").hide();
    $("#answer").click(function () {
        $("#answer").hide();
        $("#question").show();
       $("#question-acts").hide();
       $("#backcard").show();

    });

    $("#question").click(function () {
        $("#answer").show();
        $("#question").hide();
        $("#backcard").hide();
        $("#question-acts").show();
    });

   
});

// Get the animal that is specified
function GetAnimal(Animal)
{
    var loc = window.location;
    var Url = loc.protocol + "//" + loc.host  + "/Features/FlashCards/" + Animal;
    //location.href = Url;
    window.open(Url, '_self', false)
    //alert(Url);
}