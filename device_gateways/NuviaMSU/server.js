import Fastify from "fastify";
import { startGateway } from "./gateway.js";

const fastify = Fastify({
  logger: true,
});

fastify.get("/", async (request, reply) => {
  return { hello: "world" };
});

const start = async () => {
  try {
    const promises = [
        fastify.listen({ port: 3000 }),
        startGateway(),
    ];
    await Promise.all(promises);
  } catch (err) {
    fastify.log.error(err);
    process.exit(1);
  }
};
start();
