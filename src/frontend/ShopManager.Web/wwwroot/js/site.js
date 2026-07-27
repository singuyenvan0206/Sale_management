function exportTableToCSV(tableId, filename) {
    const table = document.getElementById(tableId);
    if (!table) return;

    let csv = [];
    const rows = table.querySelectorAll("tr");

    for (let i = 0; i < rows.length; i++) {
        const row = [], cols = rows[i].querySelectorAll("td, th");
        for (let j = 0; j < cols.length; j++) {
            // Remove emojis, multiple spaces, and commas from text
            let data = cols[j].innerText.replace(/(\r\n|\n|\r)/gm, "").trim().replace(/,/g, ";");
            row.push(data);
        }
        csv.push(row.join(","));
    }

    const csvFile = new Blob(["\ufeff" + csv.join("\n")], { type: "text/csv;charset=utf-8;" });
    const downloadLink = document.createElement("a");
    const url = URL.createObjectURL(csvFile);

    downloadLink.href = url;
    downloadLink.download = filename || "export.csv";
    downloadLink.style.display = "none";
    document.body.appendChild(downloadLink);
    downloadLink.click();
    document.body.removeChild(downloadLink);
}
