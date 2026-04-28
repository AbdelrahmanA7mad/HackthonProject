/* Reusable live search for GET forms across pages */
(function () {
    "use strict";

    function normalize(value) {
        return (value || "").trim();
    }

    function setupLiveSearch(form) {
        if (!form) {
            return;
        }

        var inputSelector = form.getAttribute("data-live-search-input");
        var input = inputSelector ? form.querySelector(inputSelector) : form.querySelector('input[name="searchTerm"], input[name="q"], input[type="search"], input[type="text"]');
        if (!input) {
            return;
        }

        var delay = parseInt(form.getAttribute("data-live-search-delay"), 10);
        if (Number.isNaN(delay) || delay < 0) {
            delay = 350;
        }

        var initialValue = normalize(input.value);
        var timerId;

        function submitIfChanged() {
            if (normalize(input.value) === initialValue) {
                return;
            }
            form.submit();
        }

        input.addEventListener("input", function () {
            clearTimeout(timerId);
            timerId = setTimeout(submitIfChanged, delay);
        });

        input.addEventListener("keydown", function (event) {
            if (event.key === "Enter") {
                event.preventDefault();
                clearTimeout(timerId);
                submitIfChanged();
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        var liveSearchForms = document.querySelectorAll('form[data-live-search="true"]');
        liveSearchForms.forEach(setupLiveSearch);
    });
})();
