import { MutationCache, QueryCache, QueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

function mensajeDeError(error: unknown): string {
  return error instanceof Error ? error.message : "Ocurrió un error inesperado.";
}

export const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => toast.error(mensajeDeError(error)),
  }),
  mutationCache: new MutationCache({
    onError: (error) => toast.error(mensajeDeError(error)),
  }),
});
