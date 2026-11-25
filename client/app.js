const API_BASE = "https://reviewhub-api.runasp.net";

const state = {
  kind: "",
  search: "",
  token: localStorage.getItem("rh_token") || null,
  user: JSON.parse(localStorage.getItem("rh_user") || "null"),
  authMode: "login",
  currentPlace: null,
  myRating: 0,
};

const $ = (id) => document.getElementById(id);

async function api(path, opts = {}) {
  const res = await fetch(API_BASE + path, {
    ...opts,
    headers: {
      "Content-Type": "application/json",
      ...(state.token ? { Authorization: "Bearer " + state.token } : {}),
      ...(opts.headers || {}),
    },
  });
  const data = await res.json().catch(() => ({}));
  if (!res.ok) throw new Error(data.error || "Something went wrong (" + res.status + ")");
  return data;
}

function toast(msg) {
  const t = $("toast");
  t.textContent = msg;
  t.classList.remove("hidden");
  setTimeout(() => t.classList.add("hidden"), 2600);
}

function stars(n) {
  const full = Math.round(n);
  return "★★★★★".slice(0, full) + '<span style="color:#39404f">' + "★★★★★".slice(0, 5 - full) + "</span>";
}

async function loadPlaces() {
  const params = new URLSearchParams();
  if (state.kind) params.set("kind", state.kind);
  if (state.search) params.set("search", state.search);
  const data = await api("/api/places?" + params.toString());
  const grid = $("place-grid");
  grid.innerHTML = "";
  if (data.length === 0) {
    grid.innerHTML = '<p style="color:var(--muted);grid-column:1/-1;text-align:center">Nothing found.</p>';
    return;
  }
  for (const p of data) {
    const el = document.createElement("div");
    el.className = "card";
    el.innerHTML = `
      <div class="emoji">${p.emoji}</div>
      <div class="kind">${p.kind} · ${p.category}</div>
      <h3>${p.name}</h3>
      <div class="desc">${p.description}</div>
      <div class="meta">
        <span class="stars">${stars(p.averageRating)}</span>
        <span>${p.averageRating ? p.averageRating.toFixed(1) : "—"} (${p.reviewCount})</span>
      </div>`;
    el.onclick = () => openPlace(p.id);
    grid.appendChild(el);
  }
}

async function openPlace(id) {
  try {
    const p = await api("/api/places/" + id);
    state.currentPlace = p;
    state.myRating = 0;
    $("place-detail").innerHTML = `
      <div style="font-size:2.6rem">${p.emoji}</div>
      <div class="kind" style="color:var(--accent2);font-size:.75rem;text-transform:uppercase;letter-spacing:.08em">${p.kind} · ${p.category} · ${p.meta}</div>
      <h3>${p.name}</h3>
      <p style="color:var(--muted);margin:6px 0 4px">${p.description}</p>
      <div class="stars" style="font-size:1.2rem">${stars(p.averageRating)} <span style="color:var(--muted);font-size:.85rem">${p.averageRating ? p.averageRating.toFixed(1) : "No ratings yet"} · ${p.reviews.length} review${p.reviews.length === 1 ? "" : "s"}</span></div>
      <div class="review-form">
        <b>Your review</b>
        <div class="star-input" id="star-input">
          ${[1, 2, 3, 4, 5].map((i) => `<span data-star="${i}">★</span>`).join("")}
        </div>
        <textarea id="review-text" class="input" rows="3" placeholder="What did you think?"></textarea>
        <button class="primary" id="review-submit" style="margin-top:10px">Post review</button>
      </div>
      <div id="review-list">
        ${p.reviews.length === 0 ? '<p style="color:var(--muted);margin-top:14px">Be the first to review.</p>' : ""}
        ${p.reviews.map((r) => `
          <div class="review">
            <div class="who"><b>${r.authorName}</b><span class="stars">${stars(r.rating)}</span></div>
            <p>${r.text}</p>
            <div class="who"><span>${new Date(r.createdAt).toLocaleDateString()}</span></div>
          </div>`).join("")}
      </div>`;
    $("place-modal").classList.remove("hidden");
    $("place-detail").querySelectorAll("[data-star]").forEach((s) => {
      s.onclick = () => {
        state.myRating = parseInt(s.dataset.star);
        $("place-detail").querySelectorAll("[data-star]").forEach((x) =>
          x.classList.toggle("on", parseInt(x.dataset.star) <= state.myRating));
      };
    });
    $("review-submit").onclick = submitReview;
  } catch (e) { toast(e.message); }
}

async function submitReview() {
  if (!state.token) return openAuth("login");
  if (!state.myRating) return toast("Pick a star rating first");
  try {
    await api(`/api/places/${state.currentPlace.id}/reviews`, {
      method: "POST",
      body: JSON.stringify({ rating: state.myRating, text: $("review-text").value.trim() }),
    });
    toast("Review posted! ⭐");
    openPlace(state.currentPlace.id);
    loadPlaces();
  } catch (e) {
    if (e.message.includes("401")) openAuth("login");
    else toast(e.message);
  }
}

async function showMine() {
  if (!state.token) return openAuth("login");
  $("view-home").classList.add("hidden");
  $("view-mine").classList.remove("hidden");
  const list = $("mine-list");
  list.innerHTML = "<p style='color:var(--muted)'>Loading…</p>";
  try {
    const reviews = await api("/api/reviews/mine");
    if (reviews.length === 0) {
      list.innerHTML = "<p style='color:var(--muted)'>No reviews yet. Pick a place and share your take!</p>";
      return;
    }
    list.innerHTML = reviews.map((r) => `
      <div class="review-card">
        <div class="head"><strong>${r.placeName}</strong><span class="stars">${stars(r.rating)}</span></div>
        <div class="text">${r.text || ""}</div>
        <div class="when">${new Date(r.createdAt).toLocaleDateString()}</div>
      </div>`).join("");
  } catch (e) {
    if (e.message.includes("401")) { openAuth("login"); return; }
    list.innerHTML = `<p style="color:var(--muted)">${e.message}</p>`;
  }
}

function openAuth(mode) {
  state.authMode = mode;
  $("auth-title").textContent = mode === "login" ? "Sign in" : "Create account";
  $("auth-submit").textContent = mode === "login" ? "Sign in" : "Register";
  $("auth-name").classList.toggle("hidden", mode === "login");
  $("auth-toggle").textContent = mode === "login" ? "Need an account? Register" : "Have an account? Sign in";
  $("auth-error").classList.add("hidden");
  $("auth-modal").classList.remove("hidden");
}

async function submitAuth() {
  const body = {
    name: $("auth-name").value.trim(),
    email: $("auth-email").value.trim(),
    password: $("auth-password").value,
  };
  try {
    const data = await api("/api/auth/" + state.authMode, { method: "POST", body: JSON.stringify(body) });
    state.token = data.token;
    state.user = { name: data.name, email: data.email };
    localStorage.setItem("rh_token", data.token);
    localStorage.setItem("rh_user", JSON.stringify(state.user));
    $("auth-modal").classList.add("hidden");
    updateAuthButton();
    toast("Welcome, " + data.name.split(" ")[0] + "!");
    if (state.currentPlace && !$("place-modal").classList.contains("hidden")) openPlace(state.currentPlace.id);
  } catch (e) {
    const err = $("auth-error");
    err.textContent = e.message;
    err.classList.remove("hidden");
  }
}

function updateAuthButton() {
  $("nav-auth").textContent = state.user ? state.user.name.split(" ")[0] + " ⏻" : "Sign in";
}

$("search").addEventListener("input", (e) => { state.search = e.target.value; loadPlaces(); });
document.querySelectorAll(".tab").forEach((t) => {
  t.onclick = () => {
    document.querySelectorAll(".tab").forEach((x) => x.classList.remove("active"));
    t.classList.add("active");
    state.kind = t.dataset.kind;
    loadPlaces();
  };
});
$("nav-home").onclick = () => { $("view-mine").classList.add("hidden"); $("view-home").classList.remove("hidden"); };
$("nav-mine").onclick = showMine;
$("nav-auth").onclick = () => {
  if (state.user) {
    state.token = null; state.user = null;
    localStorage.removeItem("rh_token"); localStorage.removeItem("rh_user");
    updateAuthButton(); toast("Signed out");
  } else openAuth("login");
};
$("auth-toggle").onclick = () => openAuth(state.authMode === "login" ? "register" : "login");
$("auth-submit").onclick = submitAuth;
document.querySelectorAll("[data-close]").forEach((b) => (b.onclick = () => $(b.dataset.close).classList.add("hidden")));
$("place-modal").onclick = (e) => { if (e.target.id === "place-modal") e.target.classList.add("hidden"); };
$("auth-modal").onclick = (e) => { if (e.target.id === "auth-modal") e.target.classList.add("hidden"); };

updateAuthButton();
loadPlaces();
