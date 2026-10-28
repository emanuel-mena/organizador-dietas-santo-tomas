/* Progressive enhancement for the Santo Tomás UI. No Bootstrap dependency. */
document.addEventListener("DOMContentLoaded", () => {
    const toggle = document.querySelector("[data-nav-toggle]");
    const navigation = document.querySelector("[data-navigation]");
    if (toggle && navigation) {
        const desktop = window.matchMedia("(min-width: 1024px)");
        const setExpanded = expanded => {
            toggle.setAttribute("aria-expanded", String(expanded));
            toggle.querySelector(".sr-only").textContent = expanded ? "Cerrar menú" : "Abrir menú";
            navigation.classList.toggle("hidden", !expanded);
        };
        toggle.addEventListener("click", () => setExpanded(toggle.getAttribute("aria-expanded") !== "true"));
        navigation.addEventListener("keydown", event => {
            if (event.key === "Escape" && !desktop.matches) { setExpanded(false); toggle.focus(); }
        });
        desktop.addEventListener("change", () => {
            if (!desktop.matches && navigation.contains(document.activeElement)) toggle.focus();
            setExpanded(false);
        });
    }

    const normalize = value => value.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLocaleLowerCase("es").trim();
    document.querySelectorAll("[data-search-list]").forEach(list => {
        const input = list.querySelector("[data-search-input]");
        const items = [...list.querySelectorAll("[data-search-item]")];
        const filter = () => {
            const query = normalize(input.value);
            let count = 0;
            items.forEach(item => { item.hidden = !normalize(item.dataset.searchText).includes(query); if (!item.hidden) count++; });
            list.querySelector("[data-search-count]").textContent = `${count} de ${items.length} resultados`;
            list.querySelector("[data-search-empty]").hidden = count !== 0;
            list.querySelector("[data-search-results]").hidden = count === 0;
        };
        input.addEventListener("input", filter);
        list.querySelector("[data-search-clear]").addEventListener("click", () => { input.value = ""; filter(); input.focus(); });
        filter();
    });

    const role = document.querySelector("[data-role-select]");
    const doctors = document.querySelector("[data-doctors-field]");
    if (role && doctors) {
        const update = () => {
            const visible = role.value === doctors.dataset.patientRole;
            doctors.hidden = !visible;
            doctors.disabled = !visible;
        };
        role.addEventListener("change", update);
        update();
    }

    // Keep server and client validation exposed to assistive technology.
    document.querySelectorAll(".input-validation-error").forEach(input => input.setAttribute("aria-invalid", "true"));
    const serverError = document.querySelector(".validation-summary-errors, .input-validation-error");
    if (serverError) serverError.focus();
    if (window.jQuery?.validator) {
        // Unobtrusive validation registers its parse callback with jQuery.ready.
        // Run after that callback, rather than racing it at DOMContentLoaded.
        window.jQuery(() => {
            document.querySelectorAll("form").forEach(form => {
                const validator = window.jQuery(form).data("validator");
                if (!validator) return;
                const highlight = validator.settings.highlight;
                const unhighlight = validator.settings.unhighlight;
                validator.settings.highlight = function(element, ...args) {
                    highlight?.call(this, element, ...args);
                    element.setAttribute("aria-invalid", "true");
                };
                validator.settings.unhighlight = function(element, ...args) {
                    unhighlight?.call(this, element, ...args);
                    element.removeAttribute("aria-invalid");
                };
            });
        });
    }

    const preview = document.querySelector("[data-assignment-demo]");
    if (preview) {
        const form = preview.querySelector("form");
        const status = preview.querySelector("[data-preview-status]");
        const update = () => {
            if (!form.reportValidity()) return;
            const fields = new FormData(form);
            preview.querySelector("[data-preview-patient]").textContent = fields.get("patient");
            preview.querySelector("[data-preview-diet]").textContent = fields.get("diet");
            ["start", "end"].forEach(key => {
                const time = preview.querySelector(`[data-preview-${key}]`);
                const value = fields.get(key);
                time.dateTime = value;
                time.textContent = value.split("-").reverse().join("/");
            });
            status.textContent = "Vista previa actualizada. Estos datos son de demostración y no se guardan.";
        };
        const start = form.elements.namedItem("start");
        const end = form.elements.namedItem("end");
        start.addEventListener("change", () => { end.min = start.value; });
        form.addEventListener("submit", event => { event.preventDefault(); update(); });
    }
});
