import Fastify from "fastify";
import fastifyStatic from "@fastify/static";
import path from "path";
import { fileURLToPath } from "url";
import { startGateway, reloadGateway } from "./gateway.js";
import fs from "fs/promises";

const fastify = Fastify({ logger: true });

// Get __dirname in ES modules
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Serve static files from the 'public' folder
fastify.register(fastifyStatic, {
  root: path.join(__dirname, "public"),
  prefix: "/", // Files will be served at the root
});

// JSON schema for config validation
const configSchema = {
  type: "object",
  required: ["api_url", "jobRefreshTime", "devices"],
  properties: {
    api_url: { type: "string", format: "uri" },
    jobRefreshTime: { type: "number" },
    devices: {
      type: "array",
      items: {
        type: "object",
        required: ["accessToken", "address"],
        properties: {
          accessToken: { type: "string" },
          address: { type: "string" },
        },
      },
    },
  },
};

// GET endpoint for loading current config
fastify.get("/config", async (request, reply) => {
  try {
    const data = await fs.readFile("./config/config.json", "utf8");
    return JSON.parse(data);
  } catch (error) {
    // If config file doesn't exist, return a default config
    return { api_url: "", jobRefreshTime: 0, devices: [] };
  }
});

// POST endpoint to update config
fastify.post(
  "/config",
  {
    schema: {
      body: configSchema,
      response: {
        200: {
          type: "object",
          properties: {
            success: { type: "boolean" },
            message: { type: "string" },
          },
        },
      },
    },
  },
  async (request, reply) => {
    const configData = request.body;
    try {
      await fs.writeFile(
        "./config/config.json",
        JSON.stringify(configData, null, 2)
      );
      // After updating the file, reload the gateway with the new config.
      await reloadGateway();
      return { success: true, message: "Configuration updated and gateway reloaded successfully" };
    } catch (error) {
      fastify.log.error(error);
      reply.status(500);
      return { success: false, message: "Error updating configuration" };
    }
  }
);

const start = async () => {
  try {
    const promises = [fastify.listen({ port: 3000 }), startGateway()];
    await Promise.all(promises);
  } catch (err) {
    fastify.log.error(err);
    process.exit(1);
  }
};

start();
