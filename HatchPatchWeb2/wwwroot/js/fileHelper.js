function BlazorDownloadFileFast(name, contentType, content) {
    const nameStr = BINDING.conv_string(name);
    const contentTypeStr = BINDING.conv_string(contentType);
    const contentArray = Blazor.platform.toUint8Array(content);

    const file = new File([contentArray], nameStr, { type: contentTypeStr });
    const exportUrl = URL.createObjectURL(file);

    const a = document.createElement("a");
    document.body.appendChild(a);
    a.href = exportUrl;
    a.download = nameStr;
    a.target = "_self";
    a.click();

    URL.revokeObjectURL(exportUrl);
    a.remove();
}