import { buildUrl, formatError, requestJson } from "./http.js";

export function login(baseUrl, email, password) {
  const result = requestJson(
    "POST",
    buildUrl(baseUrl, "/auth/login"),
    {
      email,
      password,
    },
    {
      tags: { name: "AuthLogin" },
    }
  );

  if (result.response.status !== 200 || !result.json) {
    throw new Error(formatError(result, "Login failed"));
  }

  return {
    baseUrl,
    email,
    password,
    accessToken: result.json.accessToken,
    refreshToken: result.json.refreshToken,
  };
}

export function refreshAccessToken(session) {
  if (!session.refreshToken) {
    throw new Error("Refresh token is missing.");
  }

  const result = requestJson(
    "POST",
    buildUrl(session.baseUrl, "/auth/refresh"),
    {
      refreshToken: session.refreshToken,
    },
    {
      tags: { name: "AuthRefresh" },
    }
  );

  if (result.response.status !== 200 || !result.json) {
    throw new Error(formatError(result, "Access token refresh failed"));
  }

  session.accessToken = result.json.accessToken;

  return session.accessToken;
}

export function authorizedJsonRequest(session, method, path, body, params = {}) {
  let result = sendAuthorizedRequest(session, method, path, body, params);

  if (result.response.status === 401) {
    refreshAccessToken(session);
    result = sendAuthorizedRequest(session, method, path, body, params);
  }

  return result;
}

function sendAuthorizedRequest(session, method, path, body, params) {
  return requestJson(method, buildUrl(session.baseUrl, path), body, {
    ...params,
    headers: {
      Authorization: `Bearer ${session.accessToken}`,
      ...(params.headers || {}),
    },
  });
}
