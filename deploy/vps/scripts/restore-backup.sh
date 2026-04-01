#!/usr/bin/env bash
set -euo pipefail

SQLCMD_BIN="/opt/mssql-tools18/bin/sqlcmd"
if [ ! -x "${SQLCMD_BIN}" ]; then
  SQLCMD_BIN="/opt/mssql-tools/bin/sqlcmd"
fi

if [ ! -x "${SQLCMD_BIN}" ]; then
  echo "[restore] sqlcmd not found in container."
  exit 1
fi

MSSQL_HOST="${MSSQL_HOST:-mssql}"
MSSQL_PORT="${MSSQL_PORT:-1433}"
MSSQL_USER="${MSSQL_USER:-sa}"
MSSQL_SA_PASSWORD="${MSSQL_SA_PASSWORD:-}"
DB_NAME="${DB_NAME:-Web_dienmay}"
BACKUP_FILE="${BACKUP_FILE:-Web_dienmay}"

if [ -z "${MSSQL_SA_PASSWORD}" ]; then
  echo "[restore] MSSQL_SA_PASSWORD is required."
  exit 1
fi

BACKUP_PATH_CONTAINER="/backup/${BACKUP_FILE}"
BACKUP_PATH_SERVER="/var/opt/mssql/backup/${BACKUP_FILE}"

if [ ! -f "${BACKUP_PATH_CONTAINER}" ]; then
  echo "[restore] Backup file not found: ${BACKUP_PATH_CONTAINER}"
  exit 1
fi

echo "[restore] Waiting for SQL Server to accept connections..."
for attempt in $(seq 1 60); do
  if "${SQLCMD_BIN}" -S "${MSSQL_HOST},${MSSQL_PORT}" -U "${MSSQL_USER}" -P "${MSSQL_SA_PASSWORD}" -C -Q "SELECT 1" >/dev/null 2>&1; then
    echo "[restore] SQL Server is ready."
    break
  fi

  if [ "${attempt}" -eq 60 ]; then
    echo "[restore] SQL Server did not become ready in time."
    exit 1
  fi

  sleep 2
done

DB_EXISTS=$("${SQLCMD_BIN}" -S "${MSSQL_HOST},${MSSQL_PORT}" -U "${MSSQL_USER}" -P "${MSSQL_SA_PASSWORD}" -C -h -1 -W -Q "SET NOCOUNT ON; SELECT DB_ID(N'${DB_NAME}');" | tr -d '\r\n[:space:]')
if [ -n "${DB_EXISTS}" ] && [ "${DB_EXISTS}" != "NULL" ]; then
  echo "[restore] Database ${DB_NAME} already exists. Skip restore."
  exit 0
fi

echo "[restore] Reading logical file names from backup ${BACKUP_FILE}..."
FILELIST=$("${SQLCMD_BIN}" -S "${MSSQL_HOST},${MSSQL_PORT}" -U "${MSSQL_USER}" -P "${MSSQL_SA_PASSWORD}" -C -h -1 -W -s "|" -Q "RESTORE FILELISTONLY FROM DISK = N'${BACKUP_PATH_SERVER}'")
DATA_LOGICAL=$(echo "${FILELIST}" | awk -F'|' '$3 == "D" {print $1; exit}')
LOG_LOGICAL=$(echo "${FILELIST}" | awk -F'|' '$3 == "L" {print $1; exit}')

if [ -z "${DATA_LOGICAL}" ] || [ -z "${LOG_LOGICAL}" ]; then
  echo "[restore] Could not determine logical data/log names from backup."
  exit 1
fi

echo "[restore] Restoring database ${DB_NAME} from backup..."
"${SQLCMD_BIN}" -S "${MSSQL_HOST},${MSSQL_PORT}" -U "${MSSQL_USER}" -P "${MSSQL_SA_PASSWORD}" -C -Q "RESTORE DATABASE [${DB_NAME}] FROM DISK = N'${BACKUP_PATH_SERVER}' WITH MOVE N'${DATA_LOGICAL}' TO N'/var/opt/mssql/data/${DB_NAME}.mdf', MOVE N'${LOG_LOGICAL}' TO N'/var/opt/mssql/data/${DB_NAME}_log.ldf', REPLACE, RECOVERY;"

echo "[restore] Restore completed."
