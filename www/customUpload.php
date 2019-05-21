
<?php
if(isset($_POST["secret"]) && isset($_POST["name"]) && isset($_POST["objData"]))
{ 
    if ( $_POST["secret"] == "87ajdf898##@@jjKJA" )
    {
        $name = $_POST["name"];
        $handle = fopen("uploadedFiles/" . $name, "w");
        if ( isset($handle) )
        {
            // Write data to the file
            $data = $_POST["objData"];
            fwrite($handle, $data);
            fflush($handle);
            fclose($handle);
        }
        else
        {
            echo "failed to write file";
        }
    }
    else
    {
        echo "invalid secret";
    }
}
else 
{
    echo "not all parameters set";
}
?>