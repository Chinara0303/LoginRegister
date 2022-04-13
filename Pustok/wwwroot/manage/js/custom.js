////$(document).ready(function () {
////    $(document).on("click", "#deletefilesweet", function (e) {
////        e.preventDefault();
////        Swal.fire({
////            title: 'Are you sure?',
////            text: "You won't be able to revert this!",
////            icon: 'warning',
////            showCancelButton: true,
////            confirmButtonColor: '#3085d6',
////            cancelButtonColor: '#d33',
////            confirmButtonText: 'Yes, delete it!'
////        }).then((result) => {
////            if (result.isConfirmed) {
////                Swal.fire(
////                    'Deleted!',
////                    'Your file has been deleted.',
////                    'success',

////                    fetch($(this).attr("href")).then(res =>
////                    {
////                        //if (res.ok) {
////                        //    alert("Silindi")
////                        //}

////                        return res.text();
////                    }).then(data => {
////                        $("#ProductTable").html(data)
////                    })
////                )
////            }
////        })
////    })
////})

$(document).ready(function () {
    $(document).on("click", "#deleteFile", function (e) {
        e.preventDefault();

        let url = $(this).attr("href");
        fetch(url).then(res =>
        {
            //if (!res.ok) {
            //    alert("test");
            //    return;
            //}

            return res.text();
        }).then(data =>
        {
            $(".updateproduct").html(data);
        })
    })
})