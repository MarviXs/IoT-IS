import { authorizedJsonRequest } from "./auth.js";
import { formatError } from "./http.js";

export function createScene(session, sceneRequest) {
  const result = authorizedJsonRequest(session, "POST", "/scenes", sceneRequest, {
    tags: { name: "CreateScene" },
  });

  if (result.response.status !== 201 || !result.json) {
    throw new Error(formatError(result, "Scene creation failed"));
  }

  return result.json;
}

export function deleteScene(session, sceneId) {
  const result = authorizedJsonRequest(
    session,
    "DELETE",
    `/scenes/${sceneId}`,
    undefined,
    {
      tags: { name: "DeleteScene" },
    }
  );

  if (result.response.status !== 204 && result.response.status !== 404) {
    throw new Error(formatError(result, `Deleting scene ${sceneId} failed`));
  }
}
