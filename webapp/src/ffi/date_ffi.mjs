/**
 * Get the day of week from Unix timestamp (milliseconds)
 * @param {number} timestamp - Unix timestamp in milliseconds
 * @returns {number} - Day of week (0 = Sunday, 1 = Monday, ..., 6 = Saturday)
 */
export function getDayOfWeek(timestamp) {
  const date = new Date(timestamp);
  return date.getUTCDay();
}
