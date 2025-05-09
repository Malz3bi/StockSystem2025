$(document).ready(function () {
    // Destroy existing DataTable instance if it exists
    if ($.fn.dataTable.isDataTable('#datanew')) {
        $('#datanew').DataTable().destroy();
    }

    // Initialize DataTable
    $('#datanew').DataTable({
        "language": {
            "sProcessing": "جاري المعالجة...",
            "sLengthMenu": "عرض _MENU_ مدخلات",
            "sZeroRecords": "لم يتم العثور على نتائج",
            "sEmptyTable": "لا توجد بيانات متاحة في الجدول",
            "sInfo": "إظهار _START_ إلى _END_ من _TOTAL_ مدخلات",
            "sInfoEmpty": "إظهار 0 إلى 0 من 0 مدخلات",
            "sInfoFiltered": "(تمت تصفيتها من _MAX_ مدخلات كليّة)",
            "sSearch": "بحث:",
            "sLoadingRecords": "جاري التحميل...",
            "oPaginate": {
                "sFirst": "الأول",
                "sPrevious": "السابق",
                "sNext": "التالي",
                "sLast": "الأخير"
            },
            "oAria": {
                "sSortAscending": ": تفعيل لترتيب العمود تصاعديًا",
                "sSortDescending": ": تفعيل لترتيب العمود تنازليًا"
            }
        },
        "destroy": true // Ensure reinitialization is allowed
    });
});






$(document).ready(function () {
    let companyCode = null;

    $.contextMenu({
        selector: '.company-row, .openmenu',
        trigger: $('.openmenu').length ? 'left' : 'right',
        callback: function (key, options) {
            const $row = options.$trigger.closest('tr');
            const rowIndex = $row.index() - 2; // Adjust for header rows
            companyCode = $(`#company-code-${rowIndex}`).text();
            const [action, followListId] = key.split(';');
            if (action === 'add') {
                $.post({
                    url: '@Url.Action("AddToFollowList")',
                    data: { followListId, companyCode, criteriaId: '@Model.Criteria?.Index', date: '@Model.SelectedDate?.ToString("yyyy-MM-dd")', viewIndex: '@Model.ViewIndex', sortColumn: '@Model.SortColumn', sortOrder: '@Model.SortOrder' },
                    success: function () { location.reload(); },
                    beforeSend: function () { $('#loadingModal').show(); },
                    complete: function () { $('#loadingModal').hide(); }
                });
            }
        },
        items: {
        @foreach(var followList in Model.FollowLists)
{
    var color = string.IsNullOrEmpty(followList.Color) ? "ffffff00" : followList.Color.Substring(3) + "75";
    @($"\"add;{followList.Id}\": {{ name: \"{followList.Name}\", icon: \"add\", style: \"background-color: #{color}\" }},")
}
        }
        });
        });