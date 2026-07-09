import axios from "axios";
import { config } from "@/utils/config.ts";

export const api = axios.create({
    baseURL: `${config.apiUrl}/api`
});
