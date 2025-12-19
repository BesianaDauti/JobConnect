function showLoader() {
    const loader = document.getElementById("global-loader");
    if (loader) loader.style.display = "flex";
}

function hideLoader() {
    const loader = document.getElementById("global-loader");
    if (loader) loader.style.display = "none";
}

document.addEventListener("DOMContentLoaded", () => {

    document.querySelectorAll("a[href]").forEach(link => {

        const url = link.getAttribute("href");

        const skip =
            !url ||
            url.startsWith("#") ||
            url.startsWith("javascript:") ||
            link.target === "_blank";

        if (skip) return;

        link.addEventListener("click", (e) => {
            showLoader(); 
        });
    });

    document.querySelectorAll("form").forEach(form => {
        form.addEventListener("submit", () => showLoader());
    });

    const originalFetch = window.fetch;
    window.fetch = async (...args) => {
        showLoader();
        try {
            const response = await originalFetch(...args);
            hideLoader();
            return response;
        } catch (err) {
            hideLoader();
            throw err;
        }
    };
    window.addEventListener("pageshow", hideLoader);
});
