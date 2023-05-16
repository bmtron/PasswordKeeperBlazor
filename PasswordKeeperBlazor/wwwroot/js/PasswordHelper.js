function buttonHelper() {
    console.log("test");
    let passContainer = $('.password-container')
    let passWordTextDiv = $('.password-div')
    let btnShowPassword = $('.btn-show-password')
    
    btnShowPassword.on('click', function() {
        let textDiv = $(this).parent().find(passWordTextDiv)//.closest(passWordTextDiv)
        var result = "";
        DotNet.invokeMethodAsync('PasswordKeeperBlazor', 'DecryptString', textDiv.text())
            .then(data => {
                result = data;
                textDiv.text(result)
                console.log(data)
            });

        console.log(result)
    });
}