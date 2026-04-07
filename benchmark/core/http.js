import http from "k6/http";

export function buildUrl(baseUrl, path) {
  const normalizedBaseUrl = baseUrl.endsWith("/")
    ? baseUrl.slice(0, -1)
    : baseUrl;
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;

  return `${normalizedBaseUrl}${normalizedPath}`;
}

export function requestJson(method, url, body, params = {}) {
  const headers = {
    Accept: "application/json",
    ...(body === undefined ? {} : { "Content-Type": "application/json" }),
    ...(params.headers || {}),
  };

  const response = http.request(
    method,
    url,
    body === undefined ? null : JSON.stringify(body),
    {
      ...params,
      headers,
    }
  );

  return {
    response,
    json: parseJsonBody(response),
  };
}

export function parseJsonBody(response) {
  if (!response.body) {
    return null;
  }

  try {
    return response.json();
  } catch (_) {
    return null;
  }
}

export function formatError(result, fallbackMessage) {
  const body =
    result && result.response && result.response.body
      ? ` Response body: ${result.response.body}`
      : "";

  return `${fallbackMessage}.${body}`;
}
