document.addEventListener("DOMContentLoaded", function () {
    const punchButton = document.getElementById("punchButton");

        punchButton.addEventListener("click", function () {
            let punchIn = localStorage.getItem("punchIn");
            let punchOut = localStorage.getItem("punchOut");

            if (!punchIn) {
                punchIn = new Date().toLocaleTimeString();
                localStorage.setItem("punchIn", punchIn);
                punchButton.textContent = "Punch Out";
            } else {
                punchOut = new Date().toLocaleTimeString();
                localStorage.setItem("punchOut", punchOut);
                punchButton.textContent = "Punch In";
            }

            setTimeout(() => {
                location.reload();
            }, 500);
        });
});

